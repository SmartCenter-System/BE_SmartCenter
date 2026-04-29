using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartCenter.Service.JwtService;

namespace SmartCenter.Api.extensions;

public static class JwtExtensions
{
    public const string AdminPolicy = "AdminPolicy";
    public const string StaffPolicy = "StaffPolicy";
    public const string UserPolicy = "UserPolicy";
    public const string StudentPolicy = "StudentPolicy";
    public const string LecturerPolicy = "LecturerPolicy";
    public const string StaffOrAdminPolicy = "StaffOrAdminPolicy";
    public const string StaffOrUserPolicy = "StaffOrUserPolicy";
    

    public static void AddJwtServices(this IServiceCollection services, IConfiguration configuration)
    {
        JwtOptions jwtOption = new JwtOptions();
        configuration.GetSection(nameof(JwtOptions)).Bind(jwtOption);
        var key = Encoding.UTF8.GetBytes(jwtOption.SecretKey);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOption.Issuer,
                    ValidAudience = jwtOption.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AdminPolicy, policy =>
                policy.RequireRole("Admin"));
            // [Authorize(Policy = JwtExtensions.AdminPolicy)]

            options.AddPolicy(StaffPolicy, policy =>
                policy.RequireRole("Staff"));
            // [Authorize(Policy = JwtExtensions.SellerPolicy)]

            options.AddPolicy(UserPolicy, policy =>
                policy.RequireRole("User"));

            options.AddPolicy(StaffOrUserPolicy, policy =>
                policy.RequireRole("Staff", "User"));
            
            options.AddPolicy(StaffOrAdminPolicy, policy =>
                policy.RequireRole("Staff", "Admin"));
            
            options.AddPolicy(StudentPolicy, policy =>
                policy.RequireRole("Student"));
            
            options.AddPolicy(LecturerPolicy, policy =>
                policy.RequireRole("Lecturer"));

            // [Authorize(Policy = JwtExtensions.SellerOrAdminPolicy)]
        });
    }
}