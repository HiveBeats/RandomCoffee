using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RandomCoffee;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly TelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, TelegramBotClient botClient, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _botClient = botClient;
        _serviceProvider = serviceProvider;
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
    
    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoffeeContext>();
        
        // Only process message updates
        if (update.Type != UpdateType.Message || update.Message.Type != MessageType.Text)
            return;

        var message = update.Message;
        // Respond only for user messages
        if (message.From == null)
            return;

        // Check if the message is the /coffee command
        if (message.Text!.Equals("/coffee", StringComparison.OrdinalIgnoreCase))
        {
            var chatId = message.Chat.Id.ToString();
            var group = await db.Groups
                .Include(x => x.Coffees)
                .ThenInclude(x => x.CoffeeParticipants)
                .FirstAsync(x => x.Id == chatId, cancellationToken);
            
            var username = message.From.Username;

            try
            {
                group.AddParticipant(username);
                await db.SaveChangesAsync(cancellationToken);
                await _botClient.SendMessage(chatId:chatId, 
                    messageThreadId: message.MessageId, 
                    text: $"Отлично, в понедельник {GetNextMonday(DateTime.UtcNow):dd.MM.yyyy} я опубликую пары в чат\n", 
                    parseMode: ParseMode.Markdown, 
                    cancellationToken: cancellationToken);
            }
            catch (ApplicationException e)
            {
                await _botClient.SendMessage(chatId:chatId, 
                    messageThreadId: message.MessageId, 
                    text: e.Message, 
                    parseMode: ParseMode.Markdown, 
                    cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                await _botClient.SendMessage(chatId:chatId,
                    messageThreadId: message.MessageId,
                    text: "Непредвиденная ошибка. Попробуйте позже.", 
                    parseMode: ParseMode.Markdown, 
                    cancellationToken: cancellationToken);
            }
        }
    }
    
    private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
    
    private static DateTime GetNextMonday(DateTime fromDate)
    {
        // Calculate the days until the next Monday
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)fromDate.DayOfWeek + 7) % 7;
        
        // If today is Monday, we want the next Monday, not today
        if (daysUntilMonday == 0)
        {
            daysUntilMonday = 7;
        }
        
        return fromDate.AddDays(daysUntilMonday);
    }

}