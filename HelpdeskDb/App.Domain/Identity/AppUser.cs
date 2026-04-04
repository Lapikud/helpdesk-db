using System.ComponentModel.DataAnnotations;
using Base.Domain.Identity;

namespace App.Domain.Identity;

public class AppUser : BaseUser<AppUserRole>
{
    public ICollection<AssetReservation>? AssetReservations { get; set; }
    
    public ICollection<AppRefreshToken>? RefreshTokens { get; set; }
}