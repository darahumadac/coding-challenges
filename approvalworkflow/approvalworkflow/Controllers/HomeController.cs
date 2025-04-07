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
        if(Request.Query.Count == 0)
        {
            var userRequests = await _requestService.GetRecordsByUserAsync(User);
            viewModel.Requests = userRequests;
            return View(viewModel);
        }

        var paginator = new Paginator<UserRequest>{
            Page = viewModel.Page, 
            PageSize = viewModel.PageSize
        };
        var paginatedUserRequests = await _requestService.GetRecordsByUserAsync(User, paginator);
        viewModel.Requests = paginatedUserRequests;
        viewModel.TotalRecords = paginator.TotalRecords;
        viewModel.Start = paginator.Start;
        viewModel.End = paginator.End;
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
