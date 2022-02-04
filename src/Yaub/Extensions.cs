namespace Yaub;

public static class Extensions
{
    public static Storage GetGuildStorage(this CommandContext context)
        => context.Services.GetRequiredService<StorageCollection>().Get($"guild/{context.Guild.Id}");
}