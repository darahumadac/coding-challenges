namespace approvalworkflow.Models;
public class StatusCodeViewModel
{
    public int StatusCode { get; set; }
    public string DangerText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Lead { get; set; } = string.Empty;

}