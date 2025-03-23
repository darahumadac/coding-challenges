using Microsoft.AspNetCore.Mvc;

namespace approvalworkflow.Controllers;

[Route("Manage")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Route("Users")]
    public IActionResult Users(){
        return View();
    }
    [Route("Requests")]
    public IActionResult Requests(){
        return View();
    }

}

