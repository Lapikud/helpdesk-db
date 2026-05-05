namespace App.DTO.v1.Identity;

public class IdentityResponse
{
    public Guid Id { get; set; }

    public string Username { get; set; } = default!;

    public List<string> Roles { get; set; } = new();
}
