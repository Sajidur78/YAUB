namespace Yaub;
using DSharpPlus.EventArgs;

public class ModerationModule
{
    public StorageCollection Storage { get; private set; }
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

        if (args.Author is DiscordMember member)
        {
            if (restricted.Contains(args.Channel.Id) && !member.Permissions.HasFlag(Permissions.ManageChannels | Permissions.ManageMessages))
            {
                await args.Message.DeleteAsync();
            }
        }
    }

    public static void Setup(IServiceProvider services, DiscordClient client)
    {
        new ModerationModule().Init(services, client);
    }
}