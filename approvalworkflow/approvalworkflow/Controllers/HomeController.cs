using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using approvalworkflow.Models;
using Microsoft.AspNetCore.Authorization;
using approvalworkflow.Services;
using System.Threading.Tasks;

namespace approvalworkflow.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepositoryService<UserRequest> _requestService;

    public HomeController(ILogger<HomeController> logger, IRepositoryService<UserRequest> requestService)
    {
        _logger = logger;
        _requestService = requestService;
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

    [HttpGet("ConfirmDelete/{requestId}")]
    public IActionResult _ConfirmDelete(int requestId)
    {
        return PartialView("_ConfirmDeletePartial", requestId);
    }
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
