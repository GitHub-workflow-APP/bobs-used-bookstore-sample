using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Amazon.S3;
using Amazon.Polly;
using BookstoreBackend.Repository;
using BookstoreBackend.Repository.Implementations;
using BookstoreBackend.Repository.OrdersInterface;
using BookstoreBackend.Repository.Implementations.OrderImplementations;
using BookstoreBackend.Repository.WelcomePageInterface;
using BookstoreBackend.Repository.Implementations.WelcomePageImplementation;
using BookstoreBackend.Notifications.NotificationsInterface;
using BookstoreBackend.Repository.Implementations.InventoryImplementation;
using BookstoreBackend.Repository.SearchImplementations;
using BookstoreBackend.Repository.Implementations.SearchImplementation;

namespace BookstoreBackend
{
    public class Startup
    {
        public Startup(IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonPolly>();
            services.AddAWSService<Amazon.Rekognition.IAmazonRekognition>();
            services.AddAWSService<Amazon.Translate.IAmazonTranslate>();
            services.AddAWSService<Amazon.CloudWatch.IAmazonCloudWatch>();

            services.AddControllersWithViews();
            services.AddDbContext<Database.DatabaseContext>(option => option.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<ISearchDatabaseCalls, SearchDatabaseCalls>();
            services.AddTransient<IExpressionFunction, ExpressionFunction>();
            services.AddTransient<IOrderDatabaseCalls, OrderDatabaseCalls>();

            services.AddTransient<IInventory, Inventory>();
            services.AddTransient<IRekognitionNPollyRepository, RekognitionNPollyRepository>();

            services.AddTransient<ISearchRepository, SearchRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IOrderDetailRepository, OrderDetailRepository>();
            services.AddTransient<IOrderStatusRepository, OrderStatusRepository>();
            

            services.AddTransient<INotifications, Notifications.Implementations.Notifications>();
            services.AddTransient<ICustomAdminPage, CustomAdmin>();
            services.AddAuthentication(options =>
            {
                // uses cookies on local machine for maintaining authentication
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                
                
            })
       .AddCookie(options =>
       {
           options.Cookie.Name = "BobsAdminCookie";
           options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
           

       })
       .AddOpenIdConnect(options =>
       {
           // sets the OpenId connect options for cognito hosted UI
           options.ResponseType = Configuration["Authentication:Cognito:ResponseType"];
           options.MetadataAddress = Configuration["Authentication:Cognito:MetadataAddress"];
           options.ClientId = Configuration["Authentication:Cognito:ClientId"];
           
           options.Authority = "https://bobsbackendbookstore.auth.us-east-1.amazoncognito.com";
           options.GetClaimsFromUserInfoEndpoint = true;
           options.TokenValidationParameters.ValidateIssuer = true;
           



       });
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
                app.UseExceptionHandler("/error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseStatusCodePagesWithReExecute("/Error/Index/{0}");

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