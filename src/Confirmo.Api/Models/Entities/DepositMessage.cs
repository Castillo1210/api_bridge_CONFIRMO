namespace Confirmo.Api.Models.Entities;

public class DepositMessage
{
    public Guid Id { get; set; }
    public Guid DepositId { get; set; }
    public string SenderType { get; set; } = string.Empty; // "user" | "system" | "finance"
    public Guid? SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty; // "text" | "image" | "status_change"
    public object? Metadata { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Deposito? Deposit { get; set; }
}