namespace Yaub.CommandModules;
using Yaub.Options;

[SlashCommandGroup("honeypot", "Commands for managing the honeypot feature", false)]
[SlashCommandPermissions(Permissions.ManageChannels | Permissions.BanMembers)]
public class HoneypotModule : ApplicationCommandModule
{
    [SlashCommand("info", "Get honeypot status")]
    public async Task GetInfo(InteractionContext context)
    {
        var storage = context.GetGuildStorage();
        var honeypot = await storage.GetHoneypot();

        if (honeypot == null)
        {
            await context.CreateResponseAsync("Status: Disabled", true);
            return;
        }

        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine($"Status: {(honeypot.Enabled ? "Enabled" : "Disabled")}");
        messageBuilder.AppendLine($"Ban Reason: {honeypot.BanReason}");
        messageBuilder.AppendLine($"Delete history: {honeypot.DeleteMessageDays} day(s)");
        messageBuilder.AppendLine();

        messageBuilder.AppendLine("Channels:");
        foreach(var channel in honeypot.Channels)
        {
            messageBuilder.AppendLine($"- <#{channel}>");
        }

        await context.CreateResponseAsync(messageBuilder.ToString(), true);
    }

    [SlashCommand("enable", "Enable the honeypot feature")]
    public async Task Enable(InteractionContext context)
    {
        var storage = context.GetGuildStorage();
        var honeypot = await storage.GetOrCreateHoneypot();

        honeypot.Enabled = true;
        await storage.Save(honeypot);
        await context.CreateResponseAsync("Honeypot enabled.", true);
    }

    [SlashCommand("disable", "Disable the honeypot feature")]
    public async Task Disable(InteractionContext context)
    {
        var storage = context.GetGuildStorage();
        var honeypot = await storage.GetOrCreateHoneypot();

        honeypot.Enabled = false;
        await storage.Save(honeypot);
        await context.CreateResponseAsync("Honeypot disabled.", true);
    }

    [SlashCommand("add", "Add a channel to the honeypot")]
    public async Task AddChannel(InteractionContext context, [Option("channel", "The channel to add")] DiscordChannel channel)
    {
        var storage = context.GetGuildStorage();
        var honeypot = await storage.GetOrCreateHoneypot();
        if (honeypot.Channels.Add(channel.Id))
        {
            await storage.Save(honeypot);
            await context.CreateResponseAsync($"Added <#{channel.Id}> to the honeypot.", true);
        }
        else
        {
            await context.CreateResponseAsync($"<#{channel.Id}> is already in the honeypot.", true);
        }
    }

    [SlashCommand("remove", "Remove a channel from the honeypot")]
    public async Task RemoveChannel(InteractionContext context, [Option("channel", "The channel to remove")] DiscordChannel channel)
    {
        var storage = context.GetGuildStorage();
        var honeypot = await storage.GetOrCreateHoneypot();
        if (honeypot.Channels.Remove(channel.Id))
        {
            await storage.Save(honeypot);
            await context.CreateResponseAsync($"Removed <#{channel.Id}> from the honeypot", true);
        }
        else
        {
            await context.CreateResponseAsync($"<#{channel.Id}> is not a honeypot", true);
        }
    }

    [SlashCommand("reason", "Set the ban reason message")]
    public async Task SetReason(InteractionContext context, [Option("reason", "The ban reason message")] string reason)
    {
        var storage = context.GetGuildStorage();
        var honeypot = await storage.GetOrCreateHoneypot();
        honeypot.BanReason = reason;

        await storage.Save(honeypot);

        await context.CreateResponseAsync($"Updated ban reason to `{reason}`", true);
    }

    [SlashCommand("days", "Set how much of the message history should be deleted")]
    public async Task SetDeleteDays(InteractionContext context, [Option("days", "Number of days")] long days) // int arguments are not supported??
    {
        var storage = context.GetGuildStorage();
        var honeypot = await storage.GetOrCreateHoneypot();
        honeypot.DeleteMessageDays = (int)days;

        await storage.Save(honeypot);

        await context.CreateResponseAsync($"Updated ban message history to {days} day(s)", true);
    }
}