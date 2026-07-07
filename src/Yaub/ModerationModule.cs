namespace Yaub;
using DSharpPlus.EventArgs;
using Yaub.CommandModules;
using Yaub.Options;

public class ModerationModule
{
    public StorageCollection Storage { get; private set; } = null!;

    public void Init(IServiceProvider services, DiscordClient client)
    {
        Storage = services.GetRequiredService<StorageCollection>();
        client.MessageCreated += OnMessageCreated;
    }

    private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
    {
        if (args.Message.Flags.HasValue && (args.Message.Flags & MessageFlags.Ephemeral) != 0)
        {
            return;
        }

        var storage = Storage.GetGuildStorage(args.Guild.Id);
        var restricted = await storage.GetOrCreate<HashSet<ulong>>(ModerationCommandsModule.RestrictedCollectionName);
        var honeypot = await storage.GetHoneypot();

        if (args.Author is DiscordMember member)
        {
            // These don't apply to moderators
            if (!member.Permissions.HasFlag(Permissions.ManageChannels | Permissions.ManageMessages))
            {
                if (restricted.Contains(args.Channel.Id))
                {
                    await args.Message.DeleteAsync();
                }

                if (honeypot?.Enabled is true && honeypot.Channels.Contains(args.Channel.Id))
                {
                    await member.BanAsync(honeypot.DeleteMessageDays, honeypot.BanReason);
                }
            }
        }
    }

    public static void Setup(IServiceProvider services, DiscordClient client)
    {
        new ModerationModule().Init(services, client);
    }
}