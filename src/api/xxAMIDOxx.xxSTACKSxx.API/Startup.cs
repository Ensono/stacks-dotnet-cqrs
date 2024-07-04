using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Amido.Stacks.API.Middleware;
using Amido.Stacks.API.Swagger.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerGen;
using xxAMIDOxx.xxSTACKSxx.API.Authentication;
using xxAMIDOxx.xxSTACKSxx.API.Authorization;
using xxAMIDOxx.xxSTACKSxx.API.Models.Requests;

namespace xxAMIDOxx.xxSTACKSxx.API;

public class Startup
{
    private readonly ILogger logger;

    private IConfiguration configuration { get; }
    private readonly IWebHostEnvironment hostingEnv;
    private readonly string pathBase;
    private readonly bool useAppInsights;
    private readonly bool useOpenTelemetry;

    private const string projectUrl = "https://github.com/amido/stacks-dotnet-cqrs";

    public Startup(IWebHostEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
    {
        this.hostingEnv = env;
        this.configuration = configuration;
        this.logger = logger;

        pathBase = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.ApiBasePathEnvName) ??
                   String.Empty;
        useAppInsights =
            !string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AppInsightsEnvName));
        useOpenTelemetry =
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.OtlpServiceName));
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // Add dependent service required by the application
    public virtual void ConfigureServices(IServiceCollection services)
    {
        if (useAppInsights)
        {
            services.AddApplicationInsightsTelemetry();
        }

        if (useOpenTelemetry)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            services.AddOpenTelemetry()
                    .WithTracing(builder =>
                    {
                        builder.ConfigureResource(resource =>
                        {
                            resource.AddService(Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.OtlpServiceName));
                        });
                        builder.AddAspNetCoreInstrumentation();
                        builder.AddConsoleExporter(options =>
                        {
                            options.Targets = ConsoleExporterOutputTargets.Debug;
                        });
                        builder.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.OltpEndpoint));
                        });
                    });
        }

        services.AddHealthChecks();

        services
            .AddMvcCore()
            .AddApiExplorer()
            .AddAuthorization()
            .AddDataAnnotations()
            .AddCors()
            /* Only required if the models will used Json.Net features for serialization
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter(typeof(CamelCaseNamingStrategy)));
            })
            */
            ;

        //Access HttpContext in ASP.NET Core: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-2.2
        services.AddHttpContextAccessor();

        // Configure Authentication
        var jwtBearerAuthenticationConfigurationSection =
            configuration.GetJwtBearerAuthenticationConfigurationSection();
        services.Configure<JwtBearerAuthenticationConfiguration>(jwtBearerAuthenticationConfigurationSection);
        var jwtBearerAuthenticationConfiguration =
            jwtBearerAuthenticationConfigurationSection.Get<JwtBearerAuthenticationConfiguration>();
        services.AddJwtBearerTokenAuthentication(jwtBearerAuthenticationConfiguration);

        services.AddSingleton<IAuthorizationPolicyProvider, ConfigurableAuthorizationPolicyProvider>();

        AddSwagger(services, jwtBearerAuthenticationConfiguration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    // Configure the pipeline with middlewares
    public virtual void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IOptions<JwtBearerAuthenticationConfiguration> jwtBearerAuthenticationOptions)
    {
        var jwtBearerAuthenticationConfiguration = jwtBearerAuthenticationOptions.Value;

        //if (!useAppInsights)
        //app.UseSerilogRequestLogging(); // Requires serilog v3 still in preview, not required when using App Insights

        app.UseCustomExceptionHandler(logger);
        app.UseCorrelationId();

        app
            .UsePathBase(pathBase)
            .UseRouting()
            //these need to be added between .UseRouting() and .UseEndpoints()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect((Environment.GetEnvironmentVariable("API_BASEPATH") ?? String.Empty) +
                                              "/swagger");
                    return Task.CompletedTask;
                });
            })

            .UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) =>
                {
                    swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{pathBase}" } };
                });
            })
            .UseSwaggerUI(c =>
            {
                c.DisplayOperationId();

                c.SwaggerEndpoint("all/swagger.json", "Menu (all)");
                c.SwaggerEndpoint("v1/swagger.json", "Menu (version 1)");
                c.SwaggerEndpoint("v2/swagger.json", "Menu (version 2)");

                if (jwtBearerAuthenticationConfiguration.HasOpenApiClient())
                {
                    c.OAuthClientId(jwtBearerAuthenticationConfiguration.OpenApi.ClientId);
                    c.OAuthUsePkce();
                }
            });
    }

    private void AddSwagger(
        IServiceCollection services,
        JwtBearerAuthenticationConfiguration jwtBearerAuthenticationConfiguration = null)
    {
        services
            //Add swagger for all endpoints without any filter
            .AddSwaggerGen(c =>
            {
                c.SwaggerDoc("all", new OpenApiInfo
                {
                    Version = "all",
                    Title = "Menu API",
                    Description = "APIs used to interact and manage menus for a restaurant",
                    Contact = new OpenApiContact()
                    {
                        Name = "Amido",
                        Url = new Uri(projectUrl),
                        Email = "stacks@amido.com"
                    },
                    //TermsOfService = new Uri("http://www.amido.com/")
                });

                //Load comments to show as examples and descriptions in the swagger page
                c.IncludeXmlComments(
                    $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{typeof(Startup).Assembly.GetName().Name}.xml");
                c.IncludeXmlComments(
                    $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{typeof(CreateMenuRequest).Assembly.GetName().Name}.xml");

                //Set default tags, shows on top, non defined tags appears at bottom
                c.DocumentFilter<SwaggerDocumentTagger>(
                    new OpenApiTag[]
                    {
                        new OpenApiTag {Name = "Menu"}, new OpenApiTag {Name = "Category"},
                        new OpenApiTag {Name = "Item"}
                    }, new string[] { });

                //By Default, all endpoints are grouped by the controller name
                //We want to Group by Api Group first, then by controller name if not provided
                c.TagActionsBy((api) => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });

                c.DocInclusionPredicate((docName, apiDesc) => { return true; });

                // Use method name as operationId
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });

                c.ConfigureForJwtBearerAuthentication(jwtBearerAuthenticationConfiguration);
            })

            //Add swagger for v1 endpoints only
            .AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Menu API",
                    Description = "APIs used to interact and manage menus for a restaurant",
                    Contact = new OpenApiContact()
                    {
                        Name = "Amido",
                        Url = new Uri(projectUrl),
                        Email = "stacks@amido.com"
                    },
                    //TermsOfService = new Uri("http://www.amido.com/")
                });

                c.IncludeXmlComments(
                    $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{this.GetType().Assembly.GetName().Name}.xml");

                // Show only operations where route starts with
                c.DocumentFilter<VersionPathFilter>("/v1");
            })

            //Add swagger for v2 endpoints only
            .AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2",
                    Title = "Menu API",
                    Description = "APIs used to interact and manage menus for a restaurant",
                    Contact = new OpenApiContact()
                    {
                        Name = "Amido",
                        Url = new Uri(projectUrl),
                        Email = "stacks@amido.com"
                    },
                    //TermsOfService = new Uri("http://www.amido.com/")
                });

                c.IncludeXmlComments(
                    $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{this.GetType().Assembly.GetName().Name}.xml");

                // Show only operations where route starts with
                c.DocumentFilter<VersionPathFilter>("/v2");
            });
    }
}

