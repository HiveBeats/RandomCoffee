using Microsoft.EntityFrameworkCore;
using RandomCoffee;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
//todo: bot token
builder.Services.AddTransient<TelegramBotClient>((sp) => new TelegramBotClient(""));
// Create the bot client
builder.Services.AddDbContext<CoffeeContext>(b =>
{
    var folder = Environment.SpecialFolder.LocalApplicationData;
    var path = Environment.GetFolderPath(folder);
    var dbPath = System.IO.Path.Join(path, "logs.db");

    b.UseSqlite($"Data Source={dbPath}");
});


var host = builder.Build();
host.Run();