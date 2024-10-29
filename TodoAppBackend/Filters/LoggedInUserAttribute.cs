using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using TodoAppBackend.Constants;

public class LoggedInUserAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (context.ActionArguments.TryGetValue(StringConstants.TODO_ID, out var todoIdObj) && todoIdObj is Guid todoId)
        {
            var dbContext = (ApplicationDbContext)context.HttpContext.RequestServices.GetService(typeof(ApplicationDbContext));
            var todo = dbContext.Todos.Find(todoId); // Changed to synchronous method

            if (todo == null || todo.UserId != userId)
            {
                context.Result = new UnauthorizedResult();
            }
        }
        else
        {
            context.Result = new BadRequestResult(); // Handle missing todoId
        }

        base.OnActionExecuting(context); // Call base method
    }
}
