using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using approvalworkflow.Models;
using Microsoft.AspNetCore.Authorization;
using approvalworkflow.Services;
using approvalworkflow.Enums;
using System.Linq.Expressions;

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
        viewModel.PageSize = Request.Query.Count > 0 ? viewModel.PageSize : 5;

        Expression<Func<UserRequest, bool>>? filter = null;

        if(viewModel.Status != null)
        {
            filter = (r) => r.Status == Enum.Parse<RequestStatus>(viewModel.Status!);
        }

        var userRequests = await _requestService.GetRecordsByUserAsync(User, viewModel, filter);
        viewModel.Requests = userRequests;
        
        if(Request.Query.Count == 0)
        {
            return View(viewModel);    
        }

        if(!HttpContext.Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
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
