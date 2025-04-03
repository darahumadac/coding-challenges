using approvalworkflow.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace approvalworkflow.Services;

public class UIService
{
    private readonly IRepositoryService<UserRequest, RequestCategory> _requestService;

    public UIService(IRepositoryService<UserRequest, RequestCategory> requestService)
    {
        _requestService = requestService;
    }

    public IEnumerable<SelectListItem> RequestCategories => _requestService.RequestCategories()
            .Select(r => new SelectListItem { Text = r.ToString(), Value = r.Id.ToString() });
}