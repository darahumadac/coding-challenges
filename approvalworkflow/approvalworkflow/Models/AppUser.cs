using System;
using System.ComponentModel.DataAnnotations;

namespace approvalworkflow.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    public AppUser? Supervisor { get; set; } = null!;
}
