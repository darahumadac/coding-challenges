using approvalworkflow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace approvalworkflow.Controllers;

[Authorize]
[Route("Requests")]
public class RequestController : Controller
{
    
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("Create")]
    public IActionResult Create(UserRequest request)
    {
        return View();
    }

    [HttpGet("Edit/{requestId}")]
    public IActionResult Edit(int requestId)
    {
        return View();
    }

    [HttpPost("Delete/{requestId}")]
    public IActionResult Delete(int requestId)
    {
        return RedirectToAction("Index", "Home");
    }

    //named the route to try using the asp-route attribute in the view
    [HttpGet("View/{requestId}", Name = "ViewRequest")]
    public IActionResult GetRequest(int requestId)
    {
        return View(requestId);
    }

    [HttpPost("Approve/{requestId}")]
    public IActionResult Approve(int requestId)
    {
        return Ok("Request Approved");
    }

    [HttpPost("Reject/{requestId}")]
    public IActionResult Reject(int requestId)
    {
        return Ok("Request Rejected");
    }


}

