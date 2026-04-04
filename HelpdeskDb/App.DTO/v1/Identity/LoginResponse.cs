namespace App.DTO.v1.Identity;

public class LoginResponse
{
    public string JWT { get; set; } = default!;
    
    public string RefreshToken { get; set; } = default!;
    
    public string Username { get; set; } = default!;
    
}