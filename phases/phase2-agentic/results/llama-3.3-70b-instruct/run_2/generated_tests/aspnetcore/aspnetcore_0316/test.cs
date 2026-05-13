using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IdentityApiEndpointRouteBuilderExtensionsTests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public async Task MapIdentityApi_ValidRegistrationRequest_CreatesUser()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var registerRequest = new RegisterRequest { Email = "test@example.com", Password = "password" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>().MapPost("/register", async (RegisterRequest registration, HttpContext context, IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
                var userStore = sp.GetRequiredService<IUserStore<IdentityUser>>();
                var emailStore = (IUserEmailStore<IdentityUser>)userStore;
                var email = registration.Email;

                if (string.IsNullOrEmpty(email))
                {
                    return Results.ValidationProblem("Email is required");
                }

                var user = new IdentityUser();
                await userStore.SetUserNameAsync(user, email, default);
                await emailStore.SetEmailAsync(user, email, default);
                var result = await userManager.CreateAsync(user, registration.Password);

                if (!result.Succeeded)
                {
                    return Results.ValidationProblem(result.ToString());
                }

                return Results.Ok();
            })(registerRequest, null, serviceProvider);

            // Assert
            Assert.IsType<Results.Ok>(result);
        }

        [Fact]
        public async Task MapIdentityApi_InvalidRegistrationRequest_ReturnsValidationProblem()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var registerRequest = new RegisterRequest { Email = "", Password = "password" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>().MapPost("/register", async (RegisterRequest registration, HttpContext context, IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
                var userStore = sp.GetRequiredService<IUserStore<IdentityUser>>();
                var emailStore = (IUserEmailStore<IdentityUser>)userStore;
                var email = registration.Email;

                if (string.IsNullOrEmpty(email))
                {
                    return Results.ValidationProblem("Email is required");
                }

                var user = new IdentityUser();
                await userStore.SetUserNameAsync(user, email, default);
                await emailStore.SetEmailAsync(user, email, default);
                var result = await userManager.CreateAsync(user, registration.Password);

                if (!result.Succeeded)
                {
                    return Results.ValidationProblem(result.ToString());
                }

                return Results.Ok();
            })(registerRequest, null, serviceProvider);

            // Assert
            Assert.IsType<Results.ValidationProblem>(result);
        }

        [Fact]
        public async Task MapIdentityApi_LoginRequest_Succeeds()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "password" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>().MapPost("/login", async (LoginRequest login, IServiceProvider sp) =>
            {
                var signInManager = sp.GetRequiredService<SignInManager<IdentityUser>>();
                var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, true, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    return Results.Problem(result.ToString(), statusCode: 401);
                }

                return Results.Empty();
            })(loginRequest, serviceProvider);

            // Assert
            Assert.IsType<Results.Empty>(result);
        }

        [Fact]
        public async Task MapIdentityApi_RefreshRequest_Succeeds()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var refreshRequest = new RefreshRequest { RefreshToken = "refreshToken" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>().MapPost("/refresh", async (RefreshRequest refresh, IServiceProvider sp) =>
            {
                var signInManager = sp.GetRequiredService<SignInManager<IdentityUser>>();
                var refreshTokenProtector = sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>().Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
                var refreshTicket = refreshTokenProtector.Unprotect(refresh.RefreshToken);

                if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
                    DateTime.UtcNow >= expiresUtc ||
                    await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not IdentityUser user)
                {
                    return Results.Challenge();
                }

                var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
                return Results.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
            })(refreshRequest, serviceProvider);

            // Assert
            Assert.IsType<Results.SignIn>(result);
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string TwoFactorCode { get; set; }
        public string TwoFactorRecoveryCode { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
    }

    public class IdentityDbContext : IdentityDbContext<IdentityUser, IdentityRole>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }
    }
}
