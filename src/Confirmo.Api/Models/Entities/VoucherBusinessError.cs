namespace Confirmo.Api.Models.Entities;

public class VoucherBusinessError
{
    public Guid Id { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string UserAction { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}