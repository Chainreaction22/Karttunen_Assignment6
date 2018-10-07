using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Homma
{
    public class ThingFilter : IActionFilter
    {

        IRepository _repos;

        public ThingFilter(IRepository repos){
            this._repos = repos;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var cache = context.HttpContext.RequestServices.GetService<IDistributedCache>();
            
            string message = context.HttpContext.Connection.RemoteIpAddress.ToString();
            string date = DateTime.Now.ToString();

            string wholeshebang = "A request from " + message + " to delete player ended at " + date;
            _repos.PostLog(wholeshebang);
            Console.WriteLine(wholeshebang);

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string message = context.HttpContext.Connection.RemoteIpAddress.ToString();
            string date = DateTime.Now.ToString();

            string wholeshebang = "A request from " + message + " to delete player started at " + date;
            _repos.PostLog(wholeshebang);
            Console.WriteLine(wholeshebang);
        }
    }
}