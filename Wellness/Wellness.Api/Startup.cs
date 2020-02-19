using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Wellness.Api.Middleware;
using Wellness.Core;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Wellness.Api
{
    public class Startup
    {
        private const string Appsettings = "AppSettings";
        private const string AlertsDbConnectionString = "SqlAlertsCloudConnection";
        private const string EventSource = "Notifications.Core.Wellness.Api";

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            LoggerFactory = loggerFactory;

            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.EventLog(EventSource, restrictedToMinimumLevel: LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(
                setupAction =>
                {
                    setupAction.EnableEndpointRouting = false;
                    setupAction.AllowEmptyInputInBodyModelBinding = true;
                    setupAction.Filters.Add(
                        new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest)
                    );
                    setupAction.Filters.Add(
                        new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError)
                    );
                }
            ).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV");

            services.AddApiVersioning(options => options.ReportApiVersions = true);
            services.AddSwaggerGen(
                options =>
                {

                    // Resolve the temprary IApiVersionDescriptionProvider service  
                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                    // Add a swagger document for each discovered API version  
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName,new OpenApiInfo()
                        {
                            Title = "Wellness Core API V" + description.ApiVersion,
                            Version = description.ApiVersion.ToString(),
                            Description = "Herbalife Wellness Service Web Api"
                        });
                    }

                    // This call remove version from parameter, without it we will have version as parameter 
                    // for all endpoints in swagger UI
                    options.OperationFilter<RemoveVersionFromParameter>();

                    // This make replacement of v{version:apiVersion} to real version of corresponding swagger doc.
                    options.DocumentFilter<ReplaceVersionWithExactValueInPath>();

                    // Tells swagger to pick up the output XML document file  
                    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

                    options.IncludeXmlComments(xmlCommentsFullPath);
                    options.DescribeAllEnumsAsStrings();
                    options.DescribeStringEnumsInCamelCase();
                });


            services.Configure<AppSettings>(Configuration.GetSection(Appsettings));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            /*
#pragma warning disable CS0618 
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
#pragma warning disable CS0618
            loggerFactory.AddEventLog(new EventLogSettings { SourceName = EventSource });
            */
#pragma warning disable CS0618 
            loggerFactory.AddSerilog();

#pragma warning restore CS0618
            app.UseLoadBalancerMiddleware();

            #region Swagger

            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
              //      options.RoutePrefix = "swagger/ui";
                    //Build a swagger endpoint for each discovered API version  
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToLowerInvariant());
                    }
                });

            #endregion

            app.UseMvc();
        }
        
        public class RemoveVersionFromParameter : IOperationFilter
        {

            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var versionParameter = operation.Parameters.Single(p => p.Name == "version");
                operation.Parameters.Remove(versionParameter);
            }
        }

        public class ReplaceVersionWithExactValueInPath : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                if (swaggerDoc == null)
                    throw new ArgumentNullException(nameof(swaggerDoc));

                var replacements = new OpenApiPaths();

                foreach (var (key, value) in swaggerDoc.Paths)
                {
                    replacements.Add(key.Replace("{version}", swaggerDoc.Info.Version, StringComparison.InvariantCulture), value);
                }

                swaggerDoc.Paths = replacements;
            }
        }
        


    }
}
