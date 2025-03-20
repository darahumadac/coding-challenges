using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace approvalworkflow.Database;

public class User : IdentityUser
{
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;

}