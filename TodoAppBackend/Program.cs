using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using TodoAppBackend.Constants;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// CORS Policy definition
var allowedHosts = builder.Configuration[StringConstants.FRONT_END_URL];
builder.Services.AddCors(options =>
{
    options.AddPolicy(StringConstants.CORS_POLICY_NAME,
        builder => builder
            .WithOrigins(allowedHosts.Split(";"))
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Entity Framework Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString(StringConstants.DATABASE_CONNECTION_STRING),
        new MySqlServerVersion(new Version(8, 0, 0))).UseOpenIddict());


builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
    options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
    options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
    options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
});

// Identity Framework Configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//Custom Attributre Filter Registered as a Service 
builder.Services.AddScoped<LoggedInUserAttribute>();


// OpenIdDict Configuration
builder.Services.AddOpenIddidctServices();


builder.Services.AddControllers();


var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();  // Authentication middleware
app.UseAuthorization();   // Authorization middleware

app.UseCors(StringConstants.CORS_POLICY_NAME);

app.MapControllers();

app.Run();
