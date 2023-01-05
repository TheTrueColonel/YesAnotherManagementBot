using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using YAMB.Context;

namespace YAMB.Modules; 

public sealed class InteractionModule : InteractionModuleBase<SocketInteractionContext> {
    private readonly DiscordSocketClient _client;
    private readonly AppDbContext _context;

    public InteractionModule(DiscordSocketClient client, AppDbContext context) {
        _client = client;
        _context = context;
    }
    
    [SlashCommand("ping", "Receive a ping message!")]
    public async Task HandlePingCommand() {
        await RespondAsync("Pong!");
    }

    [SlashCommand("ban", "Bans a user"), RequireUserPermission(GuildPermission.BanMembers)]
    public async Task HandleBasicBanCommand(IUser? user = null, ulong userId = 0) {
        if (user == null && userId == 0) {
            await RespondAsync("You must input either a user ID or select a user!");
        }

        if (user != null) {
            await Context.Guild.AddBanAsync(user);
        } else if (userId != 0) {
            await Context.Guild.AddBanAsync(userId);
        }
    }
}