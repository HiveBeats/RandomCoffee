using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RandomCoffee;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly TelegramBotClient _botClient;

    public Worker(ILogger<Worker> logger, TelegramBotClient botClient)
    {
        _logger = logger;
        _botClient = botClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start receiving updates
        _botClient.StartReceiving(UpdateHandler, ErrorHandler);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
    
    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process message updates
        if (update.Type != UpdateType.Message || update.Message.Type != MessageType.Text)
            return;

        var message = update.Message;

        // Check if the message is the /coffee command
        if (message.Text.Equals("/coffee", StringComparison.OrdinalIgnoreCase))
        {
            var username = message.From.Username;
            var timestamp = DateTime.UtcNow;

            // Save to the database
            //SaveCoffeeEntry(username, timestamp);

            // Respond to the user
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"☕ Coffee logged for {username} at {timestamp}.",
                cancellationToken: cancellationToken);
        }
    }
    
    private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }

}