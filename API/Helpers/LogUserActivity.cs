using API.Extensions;
using API.interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
  public class LogUserActivity : IAsyncActionFilter
  {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        // if the user in not logged in  return
        if(!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

        // get the user's id
        var id = resultContext.HttpContext.User.GetUserId();

        // get access to the user repository
        var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

        // get our user using the repository
        var user = await repo.GetUserByIdAsync(id);

        // modify the last active property
        user.LastActive = DateTime.UtcNow;

        // save all the changes
        await repo.SaveAllAsync();

    }
  }
}
