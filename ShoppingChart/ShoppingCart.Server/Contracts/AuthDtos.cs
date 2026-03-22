using ShoppingCartAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ShoppingCartAPI.Contracts
{
    public class RegisterRequest
    {
        [Required, MaxLength(100)]
        public string FirstName { get; init; }

        [MaxLength(100)]
        public string? MiddleName { get; init; }

        [Required, MaxLength(100)]
        public string LastName { get; init; }

        [Required, EmailAddress]
        public string Email { get; init; }

        [Required, MinLength(8), MaxLength(128)]
        public string Password { get; init; }

        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; init; }

        public string? FullName { get; init; }

    }

    public record LoginRequest(
        [Required, EmailAddress]
        string Email,
        [Required, MinLength(8), MaxLength(128)]
        string Password);

    public record AuthResponse(
        int UserId,
        string Email,
        string AccessToken,
        string RefreshToken,
        DateTime Expires
    );

    public class AuthDtos
    {
    }
}
