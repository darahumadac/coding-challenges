using System.Threading.Tasks;
using approvalworkflow.Models;
using approvalworkflow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace approvalworkflow.Controllers;

[Authorize]
[Route("Requests")]
public class RequestController : Controller
{
    private readonly IRepositoryService<UserRequest, RequestCategory> _requestService;

    public RequestController(IRepositoryService<UserRequest, RequestCategory> requestService)
    {
        _requestService = requestService;
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        var viewModel = new UserRequestViewModel
        {
            RequestCategories = _requestService.RequestCategories().Select(r => new SelectListItem { Text = r.ToString(), Value = r.Id.ToString() })
        };
        return View(viewModel);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(UserRequestViewModel request)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Something was wrong with the request");
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return View(request);
        }
        
        var userRequest = new UserRequest
        {
            Title = request.Title,
            Description = request.Description,
            TypeId =  request.RequestCategoryId,
            User = User
            // CreatedBy = currentUser //can only do this if the appUser is tracked. This will not work if this is AsNoTracking
        };
        var created = await _requestService.CreateRecordAsync(userRequest);
        if (!created)
        {
            ModelState.AddModelError(string.Empty, "Unexpected error while saving request");
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View(request);
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpPost("Save")]
    public async Task<IActionResult> Save(UserRequestViewModel request)
    {
        if (!ModelState.IsValid)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return BadRequest(ModelState);
        }

        var userRequest = new UserRequest
        {
            Id = request.Id,
            Title = request.Title,
            Description = request.Description,
            TypeId = request.RequestCategoryId
        };
        if (await _requestService.UpdateRecordAsync(userRequest))
        {
            return Ok();
        }

        return new StatusCodeResult(500);

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

