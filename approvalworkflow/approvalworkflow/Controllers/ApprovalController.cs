using Microsoft.AspNetCore.Mvc;

namespace approvalworkflow.Controllers;

public class ApprovalController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

}

