using System.Net;
using System.Net.Sockets;
namespace ChatServer;

//Логика чат-сервера: приём подключений, регистрация ников, команды и рассылка сообщений
public class ChatServerHost
{
    private const int MaxNickLength = 20;
    private readonly int _port;
    private readonly ChatLogger _logger;
    private readonly Dictionary<string, ClientSession> _clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public ChatServerHost(int port, ChatLogger logger)
    {
        _port = port;
        _logger = logger;
    }
    public async Task RunAsync(CancellationToken token)
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();
        _logger.Log($"Сервер запущен на порту {_port}. Ctrl+C - остановка.");

        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient tcp;
                try
                {
                    tcp = await listener.AcceptTcpClientAsync(token);
                }
                catch (SocketException ex)
                {
                    _logger.Log($"Ошибка при приёме подключения: {ex.Message}");
                    continue;
                }

                ClientSession session;
                try
                {
                    session = new ClientSession(tcp);
                }
                catch (Exception ex) when (ex is SocketException or ObjectDisposedException or InvalidOperationException)
                {
                    tcp.Close(); // клиент успел отключиться сразу после подключения
                    continue;
                }

                _ = HandleClientAsync(session); // каждого клиента - в свою задачу
            }
        }
        catch (OperationCanceledException)
        {
            // штатная остановка по Ctrl+C
        }
        finally
        {
            listener.Stop();
            _logger.Log("Сервер остановлен.");
        }
    }
    private async Task HandleClientAsync(ClientSession session)
    {
        string? nick = null;
        try
        {
            nick = await RegisterNickAsync(session);
            if (nick is null)
                return; // клиент отключился до регистрации

            _logger.Log($"{nick} подключился ({session.Endpoint})");
            await BroadcastAsync($"*** {nick} вошёл в чат ***");

            string? line;
            while ((line = await session.ReadLineAsync()) != null)
            {
                line = line.Trim();
                if (line.Length == 0)
                    continue;
                if (line == "/exit")
                    break;

                await ProcessMessageAsync(session, line);
            }
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        {
            // клиент внезапно оборвал соединение (например, закрыл окно)
        }
        catch (Exception ex)
        {
            _logger.Log($"Непредвиденная ошибка при обслуживании {session.Endpoint}: {ex.Message}");
        }
        finally
        {
            if (nick is not null)
            {
                lock (_lock)
                {
                    _clients.Remove(nick);
                }
                _logger.Log($"{nick} отключился");
                await BroadcastAsync($"*** {nick} покинул чат ***");
            }
            session.Dispose();
        }
    }
    //Запрашивает ник, пока клиент не пришлёт свободный и допустимый.
    //Ответы клиенту: NICK_OK, NICK_TAKEN, NICK_INVALID. Возвращает null, если клиент отключился.
    private async Task<string?> RegisterNickAsync(ClientSession session)
    {
        while (true)
        {
            string? requested = await session.ReadLineAsync();
            if (requested is null)
                return null;

            requested = requested.Trim();
            if (requested.Length == 0 || requested.Length > MaxNickLength
                || requested.Contains(' ') || requested.StartsWith('/'))
            {
                await session.SendAsync("NICK_INVALID");
                continue;
            }
            bool reserved;
            lock (_lock)
            {
                reserved = _clients.TryAdd(requested, session); // проверка уникальности (без учёта регистра)
            }

            if (!reserved)
            {
                await session.SendAsync("NICK_TAKEN");
                continue;
            }

            try
            {
                await session.SendAsync("NICK_OK");
            }
            catch
            {
                lock (_lock)
                {
                    _clients.Remove(requested); // не оставляем "призрачный" ник
                }
                throw;
            }
            //Ник становится "активным" только после NICK_OK: до этого рассылки клиенту не отправляются
            session.Nick = requested;
            return requested;
        }
    }
    private async Task ProcessMessageAsync(ClientSession sender, string line)
    {
        string nick = sender.Nick!;

        if (line == "/list")
        {
            await SendUserListAsync(sender);
            return;
        }

        if (line == "/w" || line.StartsWith("/w ", StringComparison.OrdinalIgnoreCase))
        {
            await SendPrivateAsync(sender, line);
            return;
        }

        if (line.StartsWith('/'))
        {
            await sender.SendAsync("Неизвестная команда. Доступно: /list, /w ник текст, /exit");
            return;
        }

        _logger.Log($"{nick}: {line}");
        await BroadcastAsync($"[{DateTime.Now:HH:mm:ss}] {nick}: {line}");
    }
    private async Task SendUserListAsync(ClientSession requester)
    {
        string[] nicks;
        lock (_lock)
        {
            nicks = _clients.Values
                .Where(s => s.Nick != null)
                .Select(s => s.Nick!)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        await requester.SendAsync($"Пользователей онлайн: {nicks.Length} ({string.Join(", ", nicks)})");
    }

    //Личное сообщение: /w ник текст - получает только адресат (отправитель видит копию)
    private async Task SendPrivateAsync(ClientSession sender, string line)
    {
        string[] parts = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            await sender.SendAsync("Использование: /w ник текст");
            return;
        }

        string targetNick = parts[1];
        string text = parts[2];

        ClientSession? target;
        lock (_lock)
        {
            _clients.TryGetValue(targetNick, out target);
        }

        if (target is null || target.Nick is null)
        {
            await sender.SendAsync($"Пользователь '{targetNick}' не найден.");
            return;
        }
        string time = DateTime.Now.ToString("HH:mm:ss");
        try
        {
            await target.SendAsync($"[{time}] (лично от {sender.Nick}): {text}");
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException)
        {
            await sender.SendAsync($"Не удалось доставить сообщение пользователю '{targetNick}'.");
            return;
        }

        if (target != sender)
            await sender.SendAsync($"[{time}] (лично для {target.Nick}): {text}");

        _logger.Log($"Личное сообщение {sender.Nick} -> {target.Nick}: {text}");
    }
    //Рассылает сообщение всем активным клиентам. Отвалившиеся клиенты не мешают остальным
    private async Task BroadcastAsync(string message)
    {
        List<ClientSession> snapshot;
        lock (_lock)
        {
            snapshot = _clients.Values.Where(s => s.Nick != null).ToList(); // копия под блокировкой
        }

        await Task.WhenAll(snapshot.Select(s => TrySendAsync(s, message)));
    }
    private static async Task TrySendAsync(ClientSession session, string message)
    {
        try
        {
            await session.SendAsync(message);
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException)
        {
            // клиент отвалился - его собственный обработчик удалит его из списка
        }
    }
}