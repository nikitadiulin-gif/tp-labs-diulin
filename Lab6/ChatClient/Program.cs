using System.Net.Sockets;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

const int DefaultPort = 5555;

Console.Write("Адрес сервера (Enter = localhost): ");
string host = Console.ReadLine() is { Length: > 0 } h ? h : "localhost";

Console.Write($"Порт сервера (Enter = {DefaultPort}): ");
string? portInput = Console.ReadLine();
int port = DefaultPort;
if (!string.IsNullOrWhiteSpace(portInput) && (!int.TryParse(portInput, out port) || port < 1 || port > 65535))
{
    Console.WriteLine("Ошибка: порт должен быть числом от 1 до 65535.");
    return;
}

// Подключение: понятное сообщение, если сервер недоступен
using var client = new TcpClient();
try
{
    using var connectTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    await client.ConnectAsync(host, port, connectTimeout.Token);
}
catch (SocketException ex)
{
    Console.WriteLine($"Не удалось подключиться к серверу {host}:{port}: {ex.Message}");
    return;
}
catch (OperationCanceledException)
{
    Console.WriteLine($"Сервер {host}:{port} не отвечает (таймаут 5 секунд).");
    return;
}

var stream = client.GetStream();
var reader = new StreamReader(stream);
var writer = new StreamWriter(stream) { AutoFlush = true };
//Регистрация ника: повторяем, пока сервер не примет
try
{
    while (true)
    {
        Console.Write("Ваш ник: ");
        string? raw = Console.ReadLine();
        if (raw is null)
            return;

        string candidate = raw.Trim();
        if (candidate.Length == 0)
        {
            Console.WriteLine("Ник не может быть пустым.");
            continue;
        }

        await writer.WriteLineAsync(candidate);
        string? response = await reader.ReadLineAsync();

        if (response is null)
        {
            Console.WriteLine("Сервер закрыл соединение.");
            return;
        }

        if (response == "NICK_OK")
            break;

        if (response == "NICK_TAKEN")
            Console.WriteLine("Этот ник уже занят. Введите другой.");
        else if (response == "NICK_INVALID")
            Console.WriteLine("Недопустимый ник: без пробелов, не длиннее 20 символов, не начинается с '/'.");
        else
            Console.WriteLine($"Неожиданный ответ сервера: {response}");
    }
}
catch (IOException)
{
    Console.WriteLine("Соединение с сервером потеряно во время регистрации.");
    return;
}

//Приём сообщений - параллельно с вводом
bool exiting = false;
bool connectionLost = false;

var receiveTask = Task.Run(async () =>
{
    try
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
            Console.WriteLine(line);
    }
    catch (Exception ex) when (ex is IOException or ObjectDisposedException)
    {
        // соединение оборвано или закрыто нами при выходе
    }

    if (!exiting)
    {
        connectionLost = true;
        Console.WriteLine("Соединение с сервером потеряно. Нажмите Enter для выхода.");
    }
});

Console.WriteLine("Подключено. Команды: /list, /w ник текст, /exit - выход.");
//Отправка сообщений
try
{
    while (!connectionLost)
    {
        string? msg = Console.ReadLine();
        if (msg is null || connectionLost)
            break;
        if (string.IsNullOrWhiteSpace(msg))
            continue;

        if (msg == "/exit")
            exiting = true;

        await writer.WriteLineAsync(msg);

        if (exiting)
            break;
    }
}
catch (IOException)
{
    if (!exiting)
        Console.WriteLine("Не удалось отправить сообщение: соединение потеряно.");
}

exiting = true;
client.Close();
await Task.WhenAny(receiveTask, Task.Delay(1000));