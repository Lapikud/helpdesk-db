using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DAL.DTO.Identity;

public class AppUser : IDomainId
{
    public Guid Id { get; set; }

    public string Username { get; set; } = default!;
}