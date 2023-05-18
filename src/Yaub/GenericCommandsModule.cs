namespace Yaub;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Linq;
using System.Runtime.CompilerServices;

public class GenericCommandsModule : ApplicationCommandModule
{
    public static ConditionalWeakTable<DiscordChannel, List<ulong>> ChannelTags = new();
    public const string ThreadSolvedPrefix = "✅";

    [SlashCommand("unsolve", "Mark a thread as solved")]
    public async Task MakeUnsolved(InteractionContext context)
    {
        if (context.Channel is not DiscordThreadChannel channel || context.Channel.Parent is not DiscordForumChannel parent)
        {
            await context.CreateResponseAsync("This command can only be used in a thread channel.", true);
            return;
        }

        var storage = context.GetGuildStorage();
        var tagName = await storage.Get<string>("SOLVE_TAG");

        if (string.IsNullOrEmpty(tagName))
        {
            tagName = "solved";
        }

        if (!ChannelTags.TryGetValue(context.Channel, out var tags))
        {
            tags = channel.AppliedTags.Select(x => x.Id).ToList();
            ChannelTags.Add(context.Channel, tags);
        }

        var tag = parent.AvailableTags.FirstOrDefault(x => x.Name.Equals(tagName))?.Id;
        if (tag is null)
        {
            await context.CreateResponseAsync("No solved tag found", true);
            return;
        }

        var solved = tags.Any(x => x == tag) || channel.Name.StartsWith(ThreadSolvedPrefix, StringComparison.OrdinalIgnoreCase);
        if (!solved)
        {
            await context.CreateResponseAsync("Thread is not marked as solved.", true);
            return;
        }

        try
        {
            await channel.ModifyAsync(x =>
            {
                tags.Remove(tag.Value);

                x.AppliedTags = tags;

                if (channel.Name.StartsWith(ThreadSolvedPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    x.Name = channel.Name.Substring(ThreadSolvedPrefix.Length).Trim();
                }
            });

            await context.CreateResponseAsync("Thread marked as unsolved.",  true);
        }
        catch
        {
            tags.Add(tag.Value);
            // ignore
        }
    }

    [SlashCommand("solve", "Mark a thread as solved")]
    public async Task MarkSolvedCommand(InteractionContext context)
    {
        if (context.Channel is not DiscordThreadChannel channel || context.Channel.Parent is not DiscordForumChannel parent)
        {
            await context.CreateResponseAsync("This command can only be used in a thread channel.", true);
            return;
        }

        var storage = context.GetGuildStorage();
        var tagName = await storage.Get<string>("SOLVE_TAG");

        if (string.IsNullOrEmpty(tagName))
        {
            tagName = "solved";
        }

        if (!ChannelTags.TryGetValue(context.Channel, out var tags))
        {
            tags = channel.AppliedTags.Select(x => x.Id).ToList();
            ChannelTags.Add(context.Channel, tags);
        }

        var tag = parent.AvailableTags.FirstOrDefault(x => x.Name.Equals(tagName))?.Id;
        if (tag is null)
        {
            await context.CreateResponseAsync("No solved tag found", true);
            return;
        }

        var solved = tags.Any(x => x == tag) || channel.Name.StartsWith(ThreadSolvedPrefix, StringComparison.OrdinalIgnoreCase);
        if (solved)
        {
            await context.CreateResponseAsync("Thread is already marked as solved.", true);
            return;
        }

        try
        {
            await channel.ModifyAsync(x =>
            {
                x.Name = $"{ThreadSolvedPrefix} {channel.Name}";
                var applied = new List<ulong>(tags) { tag.Value };
                x.AppliedTags = applied;
            });

            tags.Add(tag.Value);
            try
            {
                await context.CreateResponseAsync("Thread marked as solved.", true);
            }
            catch
            {
                // ignore
            }
        }
        catch
        {
            tags.Remove(tag.Value);
            // ignore
        }
    }
}