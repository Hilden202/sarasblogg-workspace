using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SarasBlogg.DAL;

namespace SarasBlogg.Infrastructure
{
    public class NavbarUsernamePageFilter : IAsyncPageFilter
    {
        private readonly UserAPIManager _userApi;

        public NavbarUsernamePageFilter(UserAPIManager userApi)
        {
            _userApi = userApi;
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
            => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var me = await _userApi.GetMeAsync(context.HttpContext.RequestAborted);
                if (me?.UserName != null &&
                    context.HandlerInstance is PageModel pageModel)
                {
                    pageModel.ViewData["NavbarUsername"] = me.UserName;
                }
            }

            await next();
        }
    }
}