using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using TodoAppBackend.Constants;

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

                options.SetTokenEndpointUris(StringConstants.TOKEN_ENDPOINT_URI)
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

        return services;
    }
}