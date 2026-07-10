namespace Confirmo.Api.Models.DTOs;

public record ChatMessageResponse(
    Guid Id,
    Guid DepositId,
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

public record VendedorMessageResponse(
    Guid Id,
    Guid VendedorId,
    string SenderType,
    Guid? SenderId,
    string Content,
    string MessageType,
    DateTimeOffset CreatedAt
);

public record VendedorChatHistoryResponse(
    List<VendedorMessageResponse> Messages,
    bool HasMore
);

public record SendVendedorMessageRequest(string Content, string? MessageType);