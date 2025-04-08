using approvalworkflow.Models;
using Microsoft.AspNetCore.Mvc;

namespace approvalworkflow.Controllers;

public class ErrorController : Controller
{
    [Route("/Error/{statusCode}")]
    public IActionResult Index(int statusCode)
    {
        var viewModel = statusCode switch
        {
            StatusCodes.Status404NotFound => new StatusCodeViewModel{
                DangerText = "Oops!", 
                Description = "Page not found.",
                Lead = "The page you're looking for doesn’t exist or has been moved."
            },
            StatusCodes.Status500InternalServerError => new StatusCodeViewModel{
                DangerText = "Uh-oh!", 
                Description = "Something went wrong.",
                Lead = "We're experiencing some technical issues. Please try again later."
            },
            StatusCodes.Status403Forbidden => new StatusCodeViewModel{
                DangerText = "Uh-oh!", 
                Description = "Forbidden access.",
                Lead = "You are not authorized to access this page."
            },
            _ => new StatusCodeViewModel{
                DangerText = "Oh dear!", 
                Description = "Something went wrong.",
                Lead = "We will check on this issue."
            }
        };
        viewModel.StatusCode = statusCode;
        return View(viewModel);
    }
}