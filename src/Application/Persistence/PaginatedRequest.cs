namespace Application.Persistence;

public record PaginatedRequest(
    int? PageSize,
    long? PageToken);