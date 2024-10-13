using FinancialApp.Areas.Identity.Data;
using FinancialApp.Data;
using FinancialApp.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);

// App Configurations
builder.Host.ConfigureAppConfiguration((context, config) =>
{
    config.SetBasePath(Directory.GetCurrentDirectory());
    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
});

// Service Configurations
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddDefaultIdentity<FinancialUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddMvc();
    
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

    builder.Services.Configure<IdentityOptions>(options =>
    {
        // Password settings 
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings 
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = false;
    });

    builder.Services.ConfigureApplicationCookie(options =>
    {
        // Cookie settings 
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);

        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        options.SlidingExpiration = true;
    });

    builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

// HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

