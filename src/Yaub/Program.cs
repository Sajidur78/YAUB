var botToken = Environment.GetEnvironmentVariable("YAUB_TOKEN");
var dbPath = Environment.GetEnvironmentVariable("YAUB_DB");
if (string.IsNullOrEmpty(dbPath))
    dbPath = "mongodb://localhost";

var collection = new StorageCollection(dbPath, "Yaub");
var services = new ServiceCollection().AddSingleton(collection);

var discord = new DiscordClient(new()
{
    AutoReconnect = true,
    TokenType = TokenType.Bot,
    Token = botToken
});

var commands = discord.UseCommandsNext(new()
{
    StringPrefixes = new[] { "!" },
    Services = services.BuildServiceProvider()
});

commands.RegisterCommands(Assembly.GetExecutingAssembly());
await discord.ConnectAsync();
await Task.Delay(-1);