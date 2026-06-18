namespace Confirmo.Api.Models.DTOs;

public record ChatMessageResponse(
    Guid Id,
    string SenderType,
    Guid? SenderId,
    string Content,
    string MessageType,
    object? Metadata,
    DateTimeOffset CreatedAt
);

public record SendDirectMessageRequest(
    Guid UserId,
    string Message,
    Guid? DepositId = null
);

public record ChatHistoryResponse(
    List<ChatMessageResponse> Messages,
    bool HasMore
);