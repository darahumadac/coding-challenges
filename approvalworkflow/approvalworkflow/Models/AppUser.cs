using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using approvalworkflow.Database;
using Microsoft.EntityFrameworkCore;

namespace approvalworkflow.Models;

[Index(nameof(AuthUserId), IsUnique = true)]
public class AppUser
{
    public int Id { get; set; }

    public string AuthUserId { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    public AppUser? Supervisor { get; set; } = null!;

    [NotMapped]
    public HashSet<string> Roles { get; set; } = new HashSet<string>();

    public List<UserRequest> UserRequests { get; set; } = null!;

    
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

}
