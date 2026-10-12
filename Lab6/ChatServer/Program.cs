using System.Net.Sockets;
using System.Text;
using ChatServer;

Console.OutputEncoding = Encoding.UTF8;

// Порт задаётся первым аргументом командной строки (по умолчанию 5555)
int port = 5555;
if (args.Length > 0 && (!int.TryParse(args[0], out port) || port < 1 || port > 65535))
{
    Console.WriteLine("Ошибка: порт должен быть числом от 1 до 65535.");
    Console.WriteLine("Пример: dotnet run --project ChatServer -- 6000");
    return;
}
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // даём серверу корректно остановиться
    cts.Cancel();
};
var logger = new ChatLogger("chat.log");
var server = new ChatServerHost(port, logger);
try
{
    await server.RunAsync(cts.Token);
}
catch (SocketException ex)
{
    Console.WriteLine($"Не удалось запустить сервер на порту {port}: {ex.Message}");
}