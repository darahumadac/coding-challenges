using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace approvalworkflow.Controllers.ActionFilters;

public class AjaxOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if(!context.HttpContext.Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }

}