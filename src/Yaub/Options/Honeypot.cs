namespace Yaub.Options;

[Serializeable]
public class Honeypot : ISerializable
{
    public static string StorageKey => "HONEYPOT_OPTIONS";

    public const string DefaultBanReason = "Banned for touching the honey.";

    public bool Enabled { get; set; } = false;
    public string BanReason { get; set; } = DefaultBanReason;
    public int DeleteMessageDays { get; set; } = 1;
    public HashSet<ulong> Channels { get; set; } = new();
}

public static class HoneypotExtensions
{
    public static async Task<Honeypot?> GetHoneypot(this Storage storage)
    {
        return await storage.Get<Honeypot>(Honeypot.StorageKey);
    }

    public static async Task<Honeypot> GetOrCreateHoneypot(this Storage storage)
    {
        return await storage.GetOrCreate<Honeypot>(Honeypot.StorageKey);
    }
}