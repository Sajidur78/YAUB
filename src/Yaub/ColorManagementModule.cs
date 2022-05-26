namespace Yaub;
using ColorContainer = List<KeyValuePair<ulong, string>>;

[Group("color")]
public class ColorManagementModule : BaseCommandModule
{
    [Command("remove"), RequireUserPermissions(Permissions.ManageRoles)]
    public async Task RemoveColorCommand(CommandContext context, string name)
    {
        name = name.Trim();

        var storage = context.GetGuildStorage();
        var container = await storage.GetOrCreate<ColorContainer>("colors");

        var removed = container.RemoveAll(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (removed == 0)
        {
            await context.RespondAsync($"{name} is not a registered colour.");
            return;
        }

        await context.RespondAsync($"Removed {name} from colour list.");
        await storage.SaveChanges();
    }

    [Command("add"), RequireUserPermissions(Permissions.ManageRoles)]
    public async Task AddColorCommand(CommandContext context, string name)
    {
        name = name.Trim();

        var storage = context.GetGuildStorage();
        var container = await storage.GetOrCreate<ColorContainer>("colors");
        var role = context.Guild.Roles.FirstOrDefault(
            x => x.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (role.Key == 0)
        {
            await context.RespondAsync($"{name} is not a role.");
            return;
        }

        if (container.Any(x => x.Key == role.Key))
        {
            await context.RespondAsync($"{name} is already registered.");
            return;
        }

        container.Add(new (role.Key, role.Value.Name));
        await storage.SaveChanges();
        await context.RespondAsync($"Added {role.Value.Name} to colours.");
    }

    [Command("setup"), RequireUserPermissions(Permissions.ManageRoles)]
    public async Task SetupColorsCommand(CommandContext context)
    {
        await context.TriggerTypingAsync();
        var storage = context.GetGuildStorage();
        if ((await storage.Get<IList>("colors"))?.Count is not null and not 0)
        {
            await context.RespondAsync("FUCK OFF!!");
            return;
        }

        var colors = new List<KeyValuePair<ulong, string>>();
        foreach (var role in context.Guild.Roles)
        {
            if (Globals.Colors.Contains(role.Value.Name))
                colors.Add(new (role.Key, role.Value.Name));
        }

        await storage.Set("colors", colors);
        await context.RespondAsync($"Found {colors.Count} colours.");

        await storage.SaveChanges();
    }

    [Command("list")]
    public async Task QueryColorsCommand(CommandContext context)
    {
        await context.TriggerTypingAsync();
        var storage = context.GetGuildStorage();
        if (!await storage.Contains("colors"))
        {
            await context.RespondAsync("Guild doesn't have colours set up.");
            return;
        }

        var colors = await storage.GetOrCreate<ColorContainer>("colors");
        var responseBuilder = new StringBuilder();
        responseBuilder.AppendLine("```");

        foreach (var color in colors)
            responseBuilder.AppendLine(color.Value);

        responseBuilder.AppendLine("```");
        await context.RespondAsync(responseBuilder.ToString());
    }

    [Command("assign"), RequireBotPermissions(Permissions.ManageRoles)]
    public async Task AssignColorCommand(CommandContext context, string colorName)
    {
        await context.TriggerTypingAsync();
        var storage = context.GetGuildStorage();
        var colors = await storage.GetOrCreate<ColorContainer>("colors");
        var member = context.Member;
        if (member == null)
            return;
            
        var colorRole = colors.FirstOrDefault(x => x.Value.Equals(colorName, StringComparison.OrdinalIgnoreCase));
        if (colorRole.Key == 0)
        {
            await context.RespondAsync($"{colorName} doesn't exist.");
            return;
        }

        var guildRole = member.Guild.GetRole(colorRole.Key);
        if (guildRole == null)
        {
            colors.RemoveAll(x => x.Key == colorRole.Key);
            await context.RespondAsync($"{colorName} doesn't exist.");
            return;
        }

        foreach (var role in member.Roles)
        {
            if (colors.Any(x => x.Key == role.Id))
                await member.RevokeRoleAsync(role);
        }

        await member.GrantRoleAsync(guildRole);
        await context.RespondAsync($"You're now {colorRole.Value.ToLower()}");
    }
}