using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1.Identity;

public class LoginRequest
{
    [MaxLength(128)]
    public string Username { get; set; } = default!;

    [MaxLength(128)]
    public string Password { get; set; } = default!;
}
