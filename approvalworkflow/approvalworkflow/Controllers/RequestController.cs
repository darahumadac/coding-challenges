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
    private readonly IRepositoryService<UserRequest, RequestApproval> _requestService;

    public RequestController(IRepositoryService<UserRequest, RequestApproval> requestService)
    {
        _requestService = requestService;
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
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
        var createdResult = await _requestService.CreateRecordAsync(userRequest);
        if (!createdResult.Success)
        {
            ModelState.AddModelError(string.Empty, createdResult.errorCode!);
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
            TypeId = request.RequestCategoryId,
            User = User
        };

        var updateResult = await _requestService.UpdateRecordAsync(userRequest);
        if (!updateResult.Success)
        {
            return StatusCode(500, new {erorCode = updateResult.errorCode!});
        }

        return Ok();
    }

    [HttpGet("Edit/{requestId}")]
    public IActionResult Edit(int requestId)
    {
        return View();
    }

    [HttpPost("Delete/{requestId}")]
    public async Task<IActionResult> Delete(int requestId)
    {
        var deleted = await _requestService.DeleteRecordAsync(requestId);
        if(deleted)
        {
            //TODO: change to ajax
            return RedirectToAction("Index", "Home");
        }

        return StatusCode(500);
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

