using Telegram.Bot.Types.Enums;

namespace RandomCoffee;

public class OutBoxMessage
{
    public int Id { get; set; }
    public required string ChatId { get; init; }
    public string? ReplyToMessageId { get; init; }
    public required string Text { get; init; }
    public ParseMode ParseMode { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }
}