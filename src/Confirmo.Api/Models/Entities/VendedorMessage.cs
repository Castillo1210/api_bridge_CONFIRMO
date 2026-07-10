namespace Confirmo.Api.Models.Entities;

public class VendedorMessage
{
    public Guid Id { get; set; }
    public Guid VendedorId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public Guid? SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Profile? Vendedor { get; set; }
}