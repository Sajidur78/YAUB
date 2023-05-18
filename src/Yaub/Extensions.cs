namespace Yaub;
using DSharpPlus.SlashCommands;

public static class Extensions
{
    public static Storage GetGuildStorage(this CommandContext context)
        => context.Services.GetRequiredService<StorageCollection>().Get($"guild/{context.Guild.Id}");

    public static Storage GetGuildStorage(this InteractionContext context)
        => context.Services.GetRequiredService<StorageCollection>().Get($"guild/{context.Guild.Id}");

    public static Storage GetGuildStorage(this StorageCollection storage, ulong guildId)
        => storage.Get($"guild/{guildId}");
}