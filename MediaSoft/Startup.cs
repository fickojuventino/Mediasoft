using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using MediaSoft.Data.Context;
using MediaSoft.Data.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.Text;
using System.Threading;
using AspNet.Security.OpenIdConnect.Primitives;
using OpenIddict.Abstractions;
using MediaSoft.Data;

namespace MediaSoft
{
    public class Startup
    {
        //static LoggerFactory object
        public static readonly LoggerFactory loggerFactory = new LoggerFactory(new[] {
              new ConsoleLoggerProvider((_, __) => true, true)
        });
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // instance is being created when this service is requested
            services.AddTransient<IPasswordHasher<Radnik>, CustomPasswordHasher>();
            services.AddTransient<IPasswordHasher<Worker>, PasswordHasherWorker>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<RadnikContext>(options =>
            {
                // connection string is in appsettings.json, under ConnectionStrings -> UserDbConnection
                options.UseSqlServer(Configuration.GetConnectionString("UserDbConnection"));
                options.UseLoggerFactory(loggerFactory);

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            // Registers an action used to configure a particular type of options.
            // IdentityOptions will be configured
            // parameter options - used to configure the options
            // QM
            services.Configure<IdentityOptions>(options =>
            {
                // Identity options -  Represents all the options you can use to configure the identity system.
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // Adds a set of common identity services to the application
            // including a default UI, token providers, and configures authentication to use identity cookies.
            services.AddDefaultIdentity<Radnik>()
                .AddUserStore<RadnikStore>()
                .AddSignInManager<SignInManager<Radnik>>();

            services.AddIdentityCore<Worker>()
                .AddUserStore<WorkerStore>()
                .AddSignInManager<SignInManager<Worker>>();

            // Register the openiddict services
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the EntityFramework Core stores and entities
                    options.UseEntityFrameworkCore()
                    .UseDbContext<RadnikContext>();
                })
                .AddServer(options =>
                {
                    // Register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    options.UseMvc();

                    // Enable the token endpoint (required to use the password flow).
                    options.EnableTokenEndpoint("/connect/token");

                    // Allow client applications to use the grant_type=password flow.
                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();

                    // During development, you can disable the HTTPS requirement.
                    options.DisableHttpsRequirement();

                    // Accept token requests that don't specify a client_id.
                    options.AcceptAnonymousClients();

                    // Registers the specified scopes as supported scopes so they can be returned as
                    // part of the discovery document.
                    // QM
                    options.RegisterScopes(OpenIdConnectConstants.Scopes.Email,
                               OpenIdConnectConstants.Scopes.Profile,
                               OpenIddictConstants.Scopes.Roles);

                    // Registers (and generates if necessary) a user-specific development certificate
                    // used to sign the JWT tokens issued by OpenIddict.
                    options.AddDevelopmentSigningCertificate();

                    options.UseJsonWebTokens();
                });
            //.AddValidation();
            // in case when tokens are not jwt

            // reference tokens
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = "Bearer";
            //    options.DefaultChallengeScheme = "Bearer";
            //});

            // removes default claims
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); 

            // settings up the schema for authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false; // default is true
                cfg.SaveToken = true;
                cfg.Authority = "https://localhost:44305/";
                cfg.IncludeErrorDetails = true;
                

                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    //ValidIssuer = Configuration["Tokens:Issuer"],
                    ValidateAudience = false,
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"])),
                    ClockSkew = TimeSpan.Zero,
                    // removes delay of token when expire
                    NameClaimType = OpenIdConnectConstants.Claims.Name,
                    RoleClaimType = OpenIdConnectConstants.Claims.Role
                };
            });

            services.AddAuthorization(options =>
            {
                // registering policies
                options.AddPolicy("NivoPristupaPolicy", policy => policy.RequireClaim("nivo_pristupa", "radnik"));
                //options.AddPolicy(Policies.OnlyManagers, policy => policy.Requirements.Add(new OnlyEmployeesRequirement()));
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            //app.UseCookiePolicy();
            app.UseHttpsRedirection();
            app.UseMvc();
            DbInit.InitAsync(app.ApplicationServices).GetAwaiter().GetResult();
        }
    }

    public class OidrExt : OpenIdConnectRequest {

        public string UserType { get; set; }
    }
}
