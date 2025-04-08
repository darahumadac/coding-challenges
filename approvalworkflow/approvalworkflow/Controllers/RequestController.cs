using approvalworkflow.Controllers.ActionFilters;
using approvalworkflow.Enums;
using approvalworkflow.Models;
using approvalworkflow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        return View(new UserRequestViewModel());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(UserRequestViewModel request)
    {
        if (!ModelState.IsValid)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return View(request);
        }
        
        var userRequest = new UserRequest
        {
            Id = request.Id,
            Title = request.Title,
            Description = request.Description,
            TypeId =  request.RequestCategoryId,
            User = User
            // CreatedBy = currentUser //can only do this if the appUser is tracked. This will not work if this is AsNoTracking
        };
        var createdResult = await _requestService.CreateRecordAsync(userRequest);
        if (!createdResult.Success)
        {
            ModelState.AddModelError(string.Empty, createdResult.ErrorEventId.ToString()!);
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View(request);
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpPost("Save")]
    [AjaxOnly]
    public async Task<IActionResult> Save(UserRequestViewModel request)
    {
        if (!ModelState.IsValid)
        {
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
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new {error = updateResult.ErrorEventId.ToString()!});
        }

        return Ok(updateResult.Data);
    }

    [HttpGet("Edit/{requestId}")]
    public IActionResult Edit(int requestId)
    {
        return View();
    }

    [HttpPost("Delete/{requestId}")]
    [AjaxOnly]
    public async Task<IActionResult> Delete(int requestId)
    {
        var deleted = await _requestService.DeleteRecordAsync(User, requestId);
        if(deleted)
        {
            return NoContent();
        }

        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    //named the route to try using the asp-route attribute in the view
    [HttpGet("View/{requestId}", Name = "ViewRequest")]
    [AjaxOnly]
    public async Task<IActionResult> GetRequest(int requestId)
    {
        var result = await _requestService.GetRecordByUserAsync(User, requestId);
        if(result.ErrorEventId != null)
        {
            if(result.ErrorEventId == ErrorEventId.UnauthorizedRequestAccess)
            {
                //we dont show that the request exists
                return NotFound();
            }
            return StatusCode(500);
        }
        
        var request = (UserRequest) result.Data!;
        return PartialView("_GetRequestPartialView", request);
        // return View(request);
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

