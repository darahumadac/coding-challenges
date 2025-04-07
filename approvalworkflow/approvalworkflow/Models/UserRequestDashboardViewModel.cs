using approvalworkflow.Services;

namespace approvalworkflow.Models;

public class UserRequestDashboardViewModel : Paginated<UserRequest>
{    
    public IEnumerable<UserRequest> Requests { get; set; } = new List<UserRequest>();

}