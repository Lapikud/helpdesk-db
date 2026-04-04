using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DAL.DTO.Identity;

public class AppRole : IDomainId
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
}