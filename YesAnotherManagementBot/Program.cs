using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YAMB.Modules;
using YAMB.Modules.AWS;

namespace YAMB
{
    public sealed class Program
    {
        static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync() {
            using var host = Host.CreateDefaultBuilder()
                                 .ConfigureServices((_, services) => {
                                     services.AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig {
                                         GatewayIntents = GatewayIntents.AllUnprivileged,
                                         AlwaysDownloadUsers = true
                                     }));
                                 }).Build();

            var test = Encryption.EncryptString("This is a really long test to test out the AES encryption method :)");
            var test2 = Encryption.DecryptString(test);
            
            await RunAsync(host);
        }

        private async Task RunAsync(IHost host) {
            using var scope = host.Services.CreateScope();
            var provider = scope.ServiceProvider;

            var client = provider.GetRequiredService<DiscordSocketClient>();

            client.Log += Logger;
            
            await client.LoginAsync(TokenType.Bot, await BotSettings.Instance.GetToken());
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Logger (LogMessage message) {
            var consoleColor = Console.ForegroundColor;
            switch (message.Severity) {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.WriteLine($"{DateTime.Now} [{message.Severity} {message.Source}: {message.Message}]");
            Console.ForegroundColor = consoleColor;
            return Task.CompletedTask;
        }
    }
}