using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Okta.AspNetCore;

namespace okta_aspnetcore_mvc_example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddAuthentication(options =>
              {
                  options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                  options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
              })
           .AddCookie( options => {
               options.Cookie.Name = "AcmedotNetCoreApp";              
               //options.AccessDeniedPath = "/Authorization/AccessDenied";
           })
           //.AddOktaMvc(new OktaMvcOptions
           //{
           //    // Replace these values with your Okta configuration
           //    OktaDomain = Configuration.GetValue<string>("Okta:OktaDomain"),
           //    ClientId = Configuration.GetValue<string>("Okta:ClientId"),
           //    ClientSecret = Configuration.GetValue<string>("Okta:ClientSecret"),
           //    //CallbackPath = Configuration.GetValue<string>("okta:CallbackPath"),
           //    PostLogoutRedirectUri = Configuration.GetValue<string>("okta:PostLogoutRedirectUri"),
           //    AuthorizationServerId = Configuration.GetValue<string>("Okta:AuthorizationServerId"),
           //    Scope = new List<string> { "openid", "profile", "email", "offline_access" }
           //});
           .AddOpenIdConnect(options => {
               //this config allows correctly named attributes to be pulled from appsettings.json
               //Configuration.GetSection("OpenIdConnect").Bind(options);

               //pulled directly from appsesttings
               options.Authority = Configuration["OktaAppSettings:issuer"];
               options.ClientId = Configuration["OktaAppSettings:webClientId"];
               options.ClientSecret = Configuration["OktaAppSettings:webClientSecret"]; // for code flow

               //used for signin auto return to app
               //options.CallbackPath = "/auth/signin-callback";
               options.CallbackPath = Configuration["OktaAppSettings:callbackPath"];
               //used for signout auto return to app
               //options.SignedOutCallbackPath = "/signout-callback-oidc";
               options.SignedOutCallbackPath = Configuration["OktaAppSettings:signedOutCallbackPath"];
               //used to specify specific redirect after returning to app
               //can also be set in OnRedirectToIdentityProviderForSignOut
               options.SignedOutRedirectUri = Configuration["OktaAppSettings:logoutRedirectUrl"];
               options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
               options.ResponseMode = "form_post";
               //this is constructed automatic, since okta is compliant 
               //options.MetadataAddress = "https://aclarke.oktapreview.com/oauth2/.well-known/openid-configuration";

               //implicit workflow
               //options.ResponseType = OpenIdConnectResponseType.IdToken;
               //options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

               //authorization code workflow
               options.ResponseType = OpenIdConnectResponseType.Code;
               //options.GetClaimsFromUserInfoEndpoint = true;
               //options.UsePkce = false;

               options.UseTokenLifetime = true;

               options.GetClaimsFromUserInfoEndpoint = false;
               options.RequireHttpsMetadata = false;
               options.SaveTokens = true;
               //options.Scope.Clear();
               options.Scope.Add("openid");
               options.Scope.Add("profile");
               //options.Scope.Add("phone");
               //options.Scope.Add("email");
               options.Scope.Add("offline_access");

               //to avoid having claims being removed by default
               //this allows sub claim to remain
               options.ClaimActions.Remove("sub");
               //need Core2.1
               //options.ClaimActions.MapAllExcept("iss", "nbf", "exp", "aud", "nonce", "iat", "c_hash");

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   NameClaimType = "name",
                   RoleClaimType = "role",
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   RequireExpirationTime = true,
                   RequireSignedTokens = true,
                   SaveSigninToken = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true
               };

               options.Events = new OpenIdConnectEvents
               {

                   OnMessageReceived = context =>
                   {
                       return Task.CompletedTask;
                   },

                   OnUserInformationReceived = context =>
                   {
                       // context.User.Remove("address");
                       return Task.FromResult(0);
                       //return Task.CompletedTask;
                   },

                   OnAuthenticationFailed = context =>
                   {
                       //context.HandleResponse();

                       //context.Response.StatusCode = 500;
                       //context.Response.ContentType = "text/plain";
                       //// if (Environment.IsDevelopment())
                       //if (Environment.IsDevelopment())
                       //{
                       //    // Debug only, in production do not share exceptions with the remote host.
                       //    return context.Response.WriteAsync(context.Exception.ToString());
                       //}
                       //return context.Response.WriteAsync("An error occurred processing your authentication.");
                       return Task.FromResult(0);
                   },

                   OnAuthorizationCodeReceived = context =>
                   {
                       return Task.FromResult(0);
                   },
                   OnTicketReceived = context =>
                   {
                       return Task.FromResult(0);
                   },

                   OnTokenValidated = context =>
                   {

                       return Task.FromResult(0);
                   },


                   OnRedirectToIdentityProvider = context =>
                   {
                       //use for integration with assurant.oktapreview
                       //context.ProtocolMessage.SetParameter("idp", "0oadt4xjwdPgIrMP00h7");
                       //use for integration with dev-assurant.oktapreview
                       //context.ProtocolMessage.SetParameter("idp", "0oae3xw7v1QmU2fpW0h7");
                       //return Task.FromResult(0);
                       return Task.FromResult(0);
                   },

                   OnRedirectToIdentityProviderForSignOut = context =>
                   {
                       //used to specify specific redirect after returning to app
                       context.Properties.RedirectUri = "/Home/PostLogout";
                       return Task.FromResult(0);
                   }
               };
           });

            services.AddControllersWithViews();
        }




        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
