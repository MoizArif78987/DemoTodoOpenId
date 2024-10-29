using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

public static class OpenIddictServiceExtensions
{
    public static IServiceCollection AddOpenIddidctServices(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                .UseDbContext<ApplicationDbContext>();
            })

            .AddServer(options =>
            {
                options.AddEphemeralEncryptionKey()
                .AddEphemeralSigningKey();

                options.SetTokenEndpointUris("api/Auth/login")
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow()
                .AcceptAnonymousClients()
                .RegisterScopes(OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.OfflineAccess, OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles)
                .UseReferenceAccessTokens()
                .UseReferenceRefreshTokens()
                .UseAspNetCore()

                .EnableTokenEndpointPassthrough();



                options.SetIdentityTokenLifetime(TimeSpan.FromHours(1));
            })

            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });


        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        //services.AddAuthorization(options =>
        //{
        //    options.AddPolicy("Bearer", policy =>
        //    {
        //        policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        //        policy.RequireAuthenticatedUser();
        //    });
        //});



        return services;
    }
}