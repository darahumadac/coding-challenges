using approvalworkflow.Services;

namespace approvalworkflow.Models;

public class UserRequestDashboardViewModel : Paginated<UserRequest>
{
    private string? _keyword;

    public IEnumerable<UserRequest> Requests { get; set; } = new List<UserRequest>();
    public string? Status { get; set; }
    public string? Keyword
    {
        get
        {
            return _keyword;
        }

        set
        {
            _keyword = value.Trim();
        }
    }

}