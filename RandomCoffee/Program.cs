using RandomCoffee;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
//todo: bot token
builder.Services.AddTransient<TelegramBotClient>((sp) => new TelegramBotClient(""));
// Create the bot client


var host = builder.Build();
host.Run();