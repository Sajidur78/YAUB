namespace Yaub;

public class GenericCommandsModule : BaseCommandModule
{
    [Command("roles")]
    public async Task QueryRolesCommand(CommandContext context)
    {
        var builder = new StringBuilder();

        builder.AppendLine("```");
        foreach (var role in context.Guild.Roles)
        {
            builder.AppendLine(role.Value.Name);
        }
        builder.AppendLine("```");
        
        await context.RespondAsync(builder.ToString());
    }
}