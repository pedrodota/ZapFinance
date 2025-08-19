using System.ComponentModel.DataAnnotations;

namespace MobileAggregator.Application.UseCases.UseCaseIn;

public class CreateUserUseCaseIn
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Document { get; set; } = string.Empty;
    
    [Required]
    public int DocumentType { get; set; }
}

public class GetUserUseCaseIn
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}

public class UpdateUserUseCaseIn
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    public bool Active { get; set; } = true;
}

public class DeleteUserUseCaseIn
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}

public class ListUsersUseCaseIn
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Filter { get; set; }
}
