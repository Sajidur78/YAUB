var botToken = Environment.GetEnvironmentVariable("YAUB_TOKEN");
var collection = new StorageCollection("mongodb://localhost", "Yaub");
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