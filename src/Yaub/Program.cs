﻿using System.Globalization;

var botToken = Environment.GetEnvironmentVariable("YAUB_TOKEN")?.Split(';') ?? Array.Empty<string>();
var dbPath = Environment.GetEnvironmentVariable("YAUB_DB");
if (string.IsNullOrEmpty(dbPath))
    dbPath = "mongodb://localhost";

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

if (botToken.Length == 0)
{
    Console.WriteLine("No bot token provided. Please set the YAUB_TOKEN environment variable.");
    return;
}

foreach (var token in botToken)
{
    var collection = new StorageCollection(dbPath, "Yaub");
    var services = new ServiceCollection().AddSingleton(collection);

    var discord = new DiscordClient(new()
    {
        AutoReconnect = true,
        TokenType = TokenType.Bot,
        Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMessages | DiscordIntents.MessageContents,
        Token = token
    });

    var commands = discord.UseCommandsNext(new()
    {
        StringPrefixes = new[] { "!" },
        Services = services.BuildServiceProvider()
    });

    var slash = discord.UseSlashCommands(new()
    {
        Services = commands.Services
    });

    slash.RegisterCommands(Assembly.GetExecutingAssembly());
    commands.RegisterCommands(Assembly.GetExecutingAssembly());

    ModerationModule.Setup(commands.Services, discord);
    await discord.ConnectAsync();
}

await Task.Delay(-1);