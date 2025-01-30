using Hangfire;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RandomCoffee;

public class Worker : BackgroundService
{
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<Worker> _logger;
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
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, cancellationToken: stoppingToken);

        RecurringJob.AddOrUpdate("post-coffee-announce", () => AnnounceCoffee(stoppingToken),
            Cron.Weekly(DayOfWeek.Monday, 12, 0));
        RecurringJob.AddOrUpdate("post-coffee-invite", () => InviteToCoffee(stoppingToken),
            Cron.Weekly(DayOfWeek.Friday, 12, 0));

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessOutboxMessages(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
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
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CoffeeContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var chatId = message.Chat.Id.ToString();
            var group = await db.Groups
                .Include(x => x.Coffees)
                .ThenInclude(x => x.CoffeeParticipants)
                .FirstAsync(x => x.Id == chatId, cancellationToken);

            var username = message.From.Username;

            try
            {
                group.AddParticipant(username);
                var text =
                    $"Отлично, в понедельник {DateTime.UtcNow.GetNext(DayOfWeek.Monday):dd.MM.yyyy} я опубликую пары в чат\n";
                db.OutBoxMessages.Add(new OutBoxMessage
                {
                    ChatId = chatId,
                    ReplyToMessageId = message.MessageId.ToString(),
                    Text = text,
                    ParseMode = ParseMode.Markdown,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (ApplicationException e)
            {
                db.OutBoxMessages.Add(new OutBoxMessage
                {
                    ChatId = chatId,
                    ReplyToMessageId = message.MessageId.ToString(),
                    Text = e.Message,
                    ParseMode = ParseMode.Markdown,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                await _botClient.SendMessage(chatId,
                    messageThreadId: message.MessageId,
                    text: "Непредвиденная ошибка. Попробуйте позже.",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
                await transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    private async Task AnnounceCoffee(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoffeeContext>();

        var groups = await db.Groups
            .Include(x => x.Coffees)
            .ThenInclude(x => x.CoffeeParticipants)
            .ToListAsync(cancellationToken);

        foreach (var group in groups)
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var announcement = group.AnnounceCoffee();
                if (string.IsNullOrWhiteSpace(announcement))
                    continue;

                db.OutBoxMessages.Add(new OutBoxMessage
                {
                    ChatId = group.Id,
                    Text = announcement,
                    ParseMode = ParseMode.Markdown,
                    CreatedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while announcing coffee for group {groupId}", group.Id);
                await transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    private async Task ProcessOutboxMessages(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoffeeContext>();

        // Fetch a limited number of unprocessed messages to avoid loading all at once
        const int batchSize = 100; // Adjust this based on performance testing
        var messagesToProcess = await db.OutBoxMessages
            .Where(x => x.ProcessedAt == null) // Filter for unprocessed messages
            .OrderBy(x => x.CreatedAt) // Order by creation time (or another field)
            .Take(batchSize) // Get only a batch of messages
            .ToListAsync(stoppingToken);

        if (messagesToProcess.Count == 0) return; // Exit early if no messages to process


        foreach (var message in messagesToProcess)
        {
            await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
            try
            {
                int? threadId = !string.IsNullOrWhiteSpace(message.ReplyToMessageId)
                    ? int.Parse(message.ReplyToMessageId)
                    : null;
                await _botClient.SendMessage(message.ChatId,
                    messageThreadId: threadId,
                    text: message.Text,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: stoppingToken);

                // Mark message as processed
                message.ProcessedAt = DateTime.UtcNow;

                _logger.LogInformation("Processed outbox message Id: {messageId} at {time}", message.Id,
                    DateTime.UtcNow);

                // Save changes to mark messages as processed
                await db.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Log any errors that occur during message processing
                _logger.LogError(ex, "Error processing message Id: {messageId}", message.Id);
                await transaction.RollbackAsync(stoppingToken);
            }
        }
    }

    private async Task InviteToCoffee(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoffeeContext>();

        var groups = await db.Groups
            .Include(x => x.Coffees)
            .ThenInclude(x => x.CoffeeParticipants)
            .ToListAsync(cancellationToken);
        
        foreach (var group in groups)
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var invite = group.InviteToNewCoffee();
                if (string.IsNullOrWhiteSpace(invite))
                    continue;

                db.OutBoxMessages.Add(new OutBoxMessage
                {
                    ChatId = group.Id,
                    Text = invite,
                    ParseMode = ParseMode.Markdown,
                    CreatedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating coffee for group {groupId}", group.Id);
                await transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    private Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram bot error");
        return Task.CompletedTask;
    }
}