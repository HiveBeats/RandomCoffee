using Hangfire;
using Hangfire.SQLite;
using Microsoft.EntityFrameworkCore;
using RandomCoffee;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
//todo: bot token
builder.Services.AddTransient<TelegramBotClient>(sp => new TelegramBotClient(""));

var folder = Environment.SpecialFolder.LocalApplicationData;
var path = Environment.GetFolderPath(folder);

builder.Services.AddDbContext<CoffeeContext>(b =>
{
    var dbPath = Path.Join(path, "coffee.db");
    b.UseSqlite($"Data Source={dbPath}");
});


var hangfireDbPath = Path.Join(path, "hangfire.db");
builder.Services.AddHangfire(config =>
    config.UseSQLiteStorage($"Data Source={hangfireDbPath}"));
builder.Services.AddHangfireServer();


var host = builder.Build();

host.Run();