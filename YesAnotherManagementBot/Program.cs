using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YAMB.Context;
using YAMB.Handlers;
using YAMB.Modules.AWS;

namespace YAMB
{
    public sealed class Program {
        private DiscordSocketClient _client;

        private InteractionService _interactionService;
        
        private IServiceProvider _services;

        public static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync() {
            _client = new DiscordSocketClient(new DiscordSocketConfig {
                GatewayIntents = GatewayIntents.AllUnprivileged,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Verbose
            });
            
            _services = new ServiceCollection()
                        .AddDbContext<AppDbContext>()
                        .AddSingleton(_client)
                        .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                        .AddSingleton<InteractionHandler>()
                        .BuildServiceProvider();
            
            _interactionService = _services.GetRequiredService<InteractionService>();
            
            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();

            _client.Log += Logger;
            _interactionService.Log += Logger;

            _client.Ready += async () => {
                await _interactionService.RegisterCommandsGloballyAsync();
            };
            
            await _client.LoginAsync(TokenType.Bot, await BotSettings.Instance.GetToken());
            await _client.StartAsync();

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