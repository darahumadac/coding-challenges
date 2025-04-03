using System;
using approvalworkflow.Enums;

namespace approvalworkflow.Models;

public class RequestApproval
{
    public int Id { get; set; }

    public UserRequest Request { get; set; } = null!;

    public int? ApproverId { get; set; }
    public AppUser? Approver { get; set; } = null!;

    public ApprovalStatus Status { get; set; }

}
