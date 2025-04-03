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
    private readonly IRepositoryService<UserRequest, RequestCategory> _requestService;
    private readonly AppUserService _appUserService;

    public HomeController(ILogger<HomeController> logger, IRepositoryService<UserRequest, RequestCategory> requestService, AppUserService appUserService)
    {
        _logger = logger;
        _requestService = requestService;
        _appUserService = appUserService;

    }

    public async Task<IActionResult> Index()
    {
        var requests = await _requestService.GetRecordsByUserAsync(User);
        return View(requests);
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
