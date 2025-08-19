namespace MobileAggregator.Application.UseCases.UseCaseOut;

public class CreateUserUseCaseOut
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public UserData? User { get; set; }
}

public class GetUserUseCaseOut
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserData? User { get; set; }
}

public class UpdateUserUseCaseOut
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserData? User { get; set; }
}

public class DeleteUserUseCaseOut
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ListUsersUseCaseOut
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<UserData> Users { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
}

public class UserData
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Document { get; set; } = string.Empty;
    public int DocumentType { get; set; }
    public bool Active { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
}
