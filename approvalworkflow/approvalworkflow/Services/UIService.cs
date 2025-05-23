using approvalworkflow.Enums;
using approvalworkflow.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace approvalworkflow.Services;

public class UIService
{
    private readonly ILookupService<RequestCategory> _categoryService;

    public UIService(ILookupService<RequestCategory> categoryService)
    {
        _categoryService = categoryService;
    }

    public IEnumerable<SelectListItem> RequestCategories => _categoryService.GetRecords()
            .Select(r => new SelectListItem { Text = r.ToString(), Value = r.Id.ToString() });

    public IEnumerable<int> PaginatorPageSizes => [5, 10, 20, 0];

    
    public IEnumerable<string> Statuses(Type type) => Enum.GetNames(type);
}