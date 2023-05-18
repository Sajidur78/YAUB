namespace Yaub;
using RestrictedChannelsContainer = HashSet<ulong>;

public class ModerationCommandsModule : ApplicationCommandModule
{
    public const string RestrictedCollectionName = "RESTRICTED_CHANNELS";

    [SlashCommand("restrict", "Makes the current channel restricted to send messages in"), SlashCommandPermissions(Permissions.ManageChannels | Permissions.ManageMessages)]
    public async Task MakeRestricted(InteractionContext context)
    {
        var storage = context.GetGuildStorage();
        var restricted = await storage.GetOrCreate<RestrictedChannelsContainer>(RestrictedCollectionName);
        if (!context.Member.Permissions.HasFlag(Permissions.ManageChannels))
        {
            await context.CreateResponseAsync("You do not have permission to restrict this channel.", true);
            return;
        }

        restricted.Add(context.Channel.Id);
        await storage.SaveChanges();
        await context.CreateResponseAsync("This channel is now restricted.", true);
    }

    [SlashCommand("unrestrict", "Makes the current channel unrestricted to send messages in"), SlashCommandPermissions(Permissions.ManageChannels | Permissions.ManageMessages)]
    public async Task MakeUnrestricted(InteractionContext context)
    {
        var storage = context.GetGuildStorage();
        var restricted = await storage.GetOrCreate<RestrictedChannelsContainer>(RestrictedCollectionName);
        if (!context.Member.Permissions.HasFlag(Permissions.ManageChannels))
        {
            await context.CreateResponseAsync("You do not have permission to unrestrict this channel.", true);
            return;
        }

        restricted.Remove(context.Channel.Id);
        await context.CreateResponseAsync("This channel is now unrestricted.", true);
    }
}