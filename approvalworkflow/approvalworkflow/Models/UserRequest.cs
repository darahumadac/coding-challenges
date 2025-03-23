
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using approvalworkflow.Database;
using approvalworkflow.Enums;

namespace approvalworkflow.Models;
public class UserRequest
{
    [Display(Name = "Request Id")]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public RequestCategory Type { get; set; } = null!;
    public RequestStatus Status { get; set; }
    public AppUser CreatedBy { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public List<RequestApproval> Approvals { get; set; } = null!;
    
}