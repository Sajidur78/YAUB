namespace Yaub;
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
            try
            {
                // AppliedTags getter throws NullReferenceException instead of returning an empty list or null
                tags = channel.AppliedTags.Select(x => x.Id).ToList();
            }
            catch
            {
                tags = new();
            }
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

        var perms = CollectPermission(channel, context.Member);
        if (context.User.Id != channel.CreatorId && !perms.HasFlag(Permissions.ManageMessages))
        {
            await context.CreateResponseAsync("Only the thread owner can mark a thread as unsolved.", true);
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
            try
            {
                // AppliedTags getter throws NullReferenceException instead of returning an empty list or null
                tags = channel.AppliedTags.Select(x => x.Id).ToList();
            }
            catch
            {
                tags = new ();
            }
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

        var perms = CollectPermission(channel, context.Member);
        if (context.User.Id != channel.CreatorId && !perms.HasFlag(Permissions.ManageThreads))
        {
            await context.CreateResponseAsync("Only the thread owner can mark a thread as solved.", true);
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

    public static Permissions CollectPermission(DiscordChannel channel, DiscordMember member)
    {
        var permission = member.Permissions;
        var c = channel;
        while (c != null)
        {
            foreach (var overwrite in c.PermissionOverwrites)
            {
                if (CheckApply(overwrite))
                {
                    permission |= overwrite.Allowed;
                    permission &= ~overwrite.Denied;
                }
            }
            try
            {
                c = c.Parent;
            }
            catch
            {
                c = null;
            }
        }

        return permission;

        bool CheckApply(DiscordOverwrite overwrite)
        {
            if (overwrite.Type == OverwriteType.Role)
            {
                return member.Roles.Any(x => x.Id == overwrite.Id);
            }

            return overwrite.Id == member.Id;
        }
    }
}