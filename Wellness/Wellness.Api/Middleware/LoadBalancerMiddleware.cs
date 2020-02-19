using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Wellness.Core;

namespace Wellness.Api.Middleware
{
    /// <summary>
    /// Middleware for load balancing (Azure compatible)
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LoadBalancerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private readonly AppSettings appSettings;

        public LoadBalancerMiddleware(RequestDelegate next, IHostingEnvironment env, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _env = env;
            this.appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var serverInfo = $"{CultureInfo.CurrentCulture.Name} | {DateTime.Now} | {Environment.MachineName}";
            var nlbLocalFilePath = Path.Combine(_env.ContentRootPath, appSettings.NlbLocalRelativePath);
            var appName = _env.ApplicationName;
            var dataSource = string.Empty;
            var status = (File.Exists(nlbLocalFilePath) && File.Exists(appSettings.NlbGlobalFilePath)) ? "--ONLINE--" : "--OFFLINE--";
            context.Response.StatusCode = status.Equals("--ONLINE--", StringComparison.InvariantCultureIgnoreCase) ? 200 : 503;
            status += (context.Request.Path.ToString().ToLower().EndsWith("nlbstatus.aspx")) ? $"|{appName}|" : string.Empty;
            await context.Response.WriteAsync(string.Format(appSettings.NlbHtmlTemplate, status, serverInfo, dataSource));
        }
    }
    [ExcludeFromCodeCoverage]
    public static class LoadBalancerExtensions
    {
        /// <summary>
        /// Adds the NLB middleware to the HTTP request pipeline.
        /// </summary>
        public static IApplicationBuilder UseLoadBalancerMiddleware(this IApplicationBuilder builder)
        {
            builder.MapWhen(context => context.Request.Path.ToString().ToLower().EndsWith("nlbcontrol.aspx") ||
                        context.Request.Path.ToString().ToLower().EndsWith("nlbstatus.aspx"),
                appLoader =>
                {
                    appLoader.UseMiddleware<LoadBalancerMiddleware>();
                });
            return builder;
        }
    }
}
