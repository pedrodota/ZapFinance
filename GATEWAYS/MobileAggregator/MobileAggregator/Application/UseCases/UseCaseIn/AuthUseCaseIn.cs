using System.ComponentModel.DataAnnotations;

namespace MobileAggregator.Application.UseCases.UseCaseIn;

public class LoginUseCaseIn
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public string? Device { get; set; }
}

public class RefreshTokenUseCaseIn
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ChangePasswordUseCaseIn
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordUseCaseIn
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
