using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureAadApplication.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AzureAadApplication
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AzureAd>(Configuration.GetSection("AzureAd"));
            //services.AddAuthentication(options =>
            //{
            //    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //}).AddJwtBearer(options =>
            //{
            //    options.Audience = Configuration["MySettings:Auth0Settings:Audience"];
            //    options.Authority = Configuration["MySettings:Auth0Settings:Authority"];
            //});
            //services.AddAuthorization(options => {
            //    options.DefaultPolicy
            //});
            services.AddAuthentication(
                opt => opt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            // Add framework services.
            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCookieAuthentication();

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = Configuration["AzureAd:ClientId"],
                Authority = string.Format(Configuration["AzureAd:AadInstance"], Configuration["AzureAd:TenantId"]),
                CallbackPath = Configuration["AzureAd:AuthCallback"]
            });

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                Authority = string.Format(Configuration["AzureAd:AadInstance"], Configuration["AzureAd:TenantId"]),
                Audience = Configuration["AzureAd:Audience"],
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["AzureAd:Domain"],
                    ValidateLifetime = true,
                },
                Events = OnSuccessfulValidation()
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();

                //app.Use(async(context, next)=> {
                //    if (context.Request.IsHttps)
                //        await next();
                //    else
                //    {
                //        var withhttps = "https://" + context.Request.Host + context.Request.Path;
                //        context.Response.Redirect(withhttps);
                //    }
                //});
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

           
        }

        private static Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents OnSuccessfulValidation()
        {
            return new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnTokenValidated = (context) =>
                {
                    context.Ticket.Principal.Identities.First().AddClaim(new Claim("AuthenticationType", "OAuth2Bearer"));
                    return Task.FromResult(0);
                }
            };
        }


    }

    public static class AuthenticationConfiguration
    {
        private static void ConfigurationAuthentication(this IApplicationBuilder app)
        {
            
        }
    }
}
