using DreysStoreEcommerce.Data;
using DreysStoreEcommerce.Hubs;
using DreysStoreEcommerce.Hubs.DreysStoreEcommerce.Hubs;
using DreysStoreEcommerce.Mapping;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Configure DB & Identity
// -----------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// -----------------------------
// External Authentication (Google, Facebook)
// -----------------------------
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    });

// Stripe
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// -----------------------------
// Add MVC, Razor, SignalR
// -----------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// -----------------------------
// Custom Services
// -----------------------------
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<WishlistService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddHttpContextAccessor();

// -----------------------------
// Logging
// -----------------------------
builder.Logging.ClearProviders();
builder.Services.AddLogging(configure => configure.AddConsole().AddDebug());

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

// -----------------------------
// AutoMapper
// -----------------------------
builder.Services.AddAutoMapper(typeof(Program));

// -----------------------------
// Cookie paths
// -----------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// -----------------------------
// Configure SignalR to use UserIdentifier = ApplicationUser.Id
// (Add NameIdentifier claim if missing)
// -----------------------------
builder.Services.PostConfigure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserIdClaimType = System.Security.Claims.ClaimTypes.NameIdentifier;
});

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.Events.OnValidatePrincipal = async ctx =>
        {
            var userManager = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(ctx.Principal);
            if (user != null)
            {
                var identity = ctx.Principal.Identity as System.Security.Claims.ClaimsIdentity;
                if (identity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) == null)
                {
                    identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id));
                }
            }
        };
    });

var app = builder.Build();

// -----------------------------
// Migrate DB & seed roles/admin
// -----------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await DbInitializer.Seed(context, userManager, roleManager);
}

// -----------------------------
// Middleware
// -----------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// -----------------------------
// Map routes & hubs
// -----------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<NotificationHub>("/notificationHub");

// -----------------------------
app.Run();
