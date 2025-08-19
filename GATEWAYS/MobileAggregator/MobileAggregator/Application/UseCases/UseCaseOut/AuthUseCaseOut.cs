namespace MobileAggregator.Application.UseCases.UseCaseOut;

public class LoginUseCaseOut
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public long ExpiresIn { get; set; }
    public UserInfo? User { get; set; }
}

public class RefreshTokenUseCaseOut
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public long ExpiresIn { get; set; }
}

public class ChangePasswordUseCaseOut
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ForgotPasswordUseCaseOut
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public bool Active { get; set; }
}
