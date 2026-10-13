using System.Net.Sockets;
namespace ChatServer;

//Подключённый клиент: сокет, потоки чтения/записи и ник
public class ClientSession : IDisposable
{
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private volatile string? _nick;
    public string Endpoint { get; }
    //Ник клиента; null, пока регистрация не завершена
    public string? Nick
    {
        get => _nick;
        set => _nick = value;
    }
    public ClientSession(TcpClient client)
    {
        _client = client;
        Endpoint = client.Client.RemoteEndPoint?.ToString() ?? "неизвестно";

        NetworkStream stream = client.GetStream();
        _reader = new StreamReader(stream);
        _writer = new StreamWriter(stream) { AutoFlush = true };
    }
    public Task<string?> ReadLineAsync() => _reader.ReadLineAsync();
    //Отправляет строку клиенту. Одновременные отправки из разных задач выстраиваются в очередь
    public async Task SendAsync(string message)
    {
        await _sendLock.WaitAsync();
        try
        {
            await _writer.WriteLineAsync(message);
        }
        finally
        {
            _sendLock.Release();
        }
    }
    public void Dispose() => _client.Close();
}