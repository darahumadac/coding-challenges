using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using approvalworkflow.Models;
using Microsoft.AspNetCore.Authorization;
using approvalworkflow.Services;

namespace approvalworkflow.Controllers;

[Authorize(Roles = "Requestor, Approver")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepositoryService<UserRequest, RequestApproval> _requestService;

    public HomeController(ILogger<HomeController> logger, 
            IRepositoryService<UserRequest, RequestApproval> requestService)
    {
        _logger = logger;
        _requestService = requestService;
    }

    public async Task<IActionResult> Index([FromQuery] UserRequestDashboardViewModel viewModel)
    {
        var paginator = new Paginator<UserRequest>
        {
            Page = viewModel.Page,
            PageSize = Request.Query.Count > 0 ? viewModel.PageSize : 5
        };

        var userRequests = await _requestService.GetRecordsByUserAsync(User, paginator);
        viewModel.Requests = userRequests;
        viewModel.TotalRecords = paginator.TotalRecords;
        viewModel.Start = paginator.Start;
        viewModel.End = paginator.End;
        viewModel.Page = paginator.Page;
        viewModel.PageSize = paginator.PageSize;

        if(Request.Query.Count == 0)
        {
            return View(viewModel);    
        }

        return PartialView("_TablePartial", viewModel);
    }

    [Authorize(Roles = "Approver")]
    public async Task<IActionResult> Approval()
    {
        var forApproval = await _requestService.GetRecordsForUserAsync(User);
        return View(forApproval);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
