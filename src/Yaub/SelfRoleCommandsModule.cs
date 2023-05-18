namespace Yaub;
using SelfRoleContainer = List<KeyValuePair<ulong, string>>;

public class SelfRoleCommandsModule : ApplicationCommandModule
{
    [SlashCommand("iamnot", "Remove a role from yourself")]
    public async Task UnAssignSelfRole(InteractionContext context, [Option("role", "Role to remove")] string roleName)
    {
        var storage = context.GetGuildStorage();
        var container = await storage.GetOrCreate<SelfRoleContainer>(ColorManagementModule.CollectionName);

        if (container?.Count is null or 0)
        {
            await context.CreateResponseAsync("No self-assignable roles have been set up.", true);
            return;
        }

        var role = container.FirstOrDefault(x => x.Value.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        if (role.Key == 0)
        {
            await context.CreateResponseAsync($"{roleName} is not a self-assignable role.", true);
            return;
        }

        var guildRole = context.Guild.GetRole(role.Key);
        if (guildRole is null)
        {
            await context.CreateResponseAsync($"{roleName} is not a self-assignable role.", true);
            return;
        }

        await context.Member.RevokeRoleAsync(guildRole);
        await context.CreateResponseAsync($"You have been removed from the {roleName} role.", true);
    }

    [SlashCommand("iam", "Assign yourself a role")]
    public async Task AssignSelfRole(InteractionContext context, [Option("role", "Role to assign")] string roleName)
    {
        var storage = context.GetGuildStorage();
        var container = await storage.GetOrCreate<SelfRoleContainer>(ColorManagementModule.CollectionName);

        if (container?.Count is null or 0)
        {
            await context.CreateResponseAsync("No self-assignable roles have been set up.", true);
            return;
        }

        var role = container.FirstOrDefault(x => x.Value.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        if (role.Key == 0)
        {
            await context.CreateResponseAsync($"{roleName} is not a self-assignable role.", true);
            return;
        }

        var guildRole = context.Guild.GetRole(role.Key);
        if (guildRole is null)
        {
            await context.CreateResponseAsync($"{roleName} is not a self-assignable role.", true);
            return;
        }

        await context.Member.GrantRoleAsync(guildRole);
        await context.CreateResponseAsync($"You have been granted the {roleName} role.", true);
    }
}