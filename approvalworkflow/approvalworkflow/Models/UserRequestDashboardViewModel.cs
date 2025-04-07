using approvalworkflow.Services;

namespace approvalworkflow.Models;

public class UserRequestDashboardViewModel
{
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public int Page { get; set; }
    public int TotalRecords { get; set; }

    public int Start { get; set; }
    public int End { get; set; }
    public IEnumerable<UserRequest> Requests { get; set; } = new List<UserRequest>();

}