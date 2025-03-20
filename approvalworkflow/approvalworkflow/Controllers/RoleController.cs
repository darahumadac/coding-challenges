using Microsoft.AspNetCore.Mvc;

namespace approvalworkflow.Controllers;

public class RoleController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

}

