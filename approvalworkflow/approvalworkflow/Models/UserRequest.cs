
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using approvalworkflow.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace approvalworkflow.Models;
public class UserRequest
{
    [Display(Name = "Request Id")]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    //Foreign Key
    public int TypeId { get; set; }
    //Navigation Property
    public RequestCategory Type { get; set; } = null!;
    public RequestStatus Status { get; set; }

    //Foreign Key
    public int CreatedById { get; set; }
    //Navigation Property
    public AppUser CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public List<RequestApproval> Approvals { get; set; } = null!;

}

// public record UserRequestViewModel(string Title, string Description, RequestType Type);
public class UserRequestViewModel
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    public int RequestCategoryId { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> RequestCategories { get; set; } = null!;
}