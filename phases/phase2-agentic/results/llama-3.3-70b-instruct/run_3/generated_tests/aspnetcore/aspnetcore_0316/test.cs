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
        public async Task MapIdentityApi_WithValidRegistrationRequest_CreatesUser()
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
                var user = new IdentityUser { UserName = registration.Email, Email = registration.Email };
                var result = await userManager.CreateAsync(user, registration.Password);
                return result.Succeeded ? TypedResults.Ok() : TypedResults.ValidationProblem(result);
            })(registerRequest, new DefaultHttpContext(), serviceProvider);

            // Assert
            Assert.IsType<Ok>(result);
        }

        [Fact]
        public async Task MapIdentityApi_WithInvalidRegistrationRequest_ReturnsValidationProblem()
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
                var user = new IdentityUser { UserName = registration.Email, Email = registration.Email };
                var result = await userManager.CreateAsync(user, registration.Password);
                return result.Succeeded ? TypedResults.Ok() : TypedResults.ValidationProblem(result);
            })(registerRequest, new DefaultHttpContext(), serviceProvider);

            // Assert
            Assert.IsType<ValidationProblem>(result);
        }

        [Fact]
        public async Task MapIdentityApi_WithValidLoginRequest_SignsInUser()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "password" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>().MapPost("/login", async (LoginRequest login, [FromServices] IServiceProvider sp) =>
            {
                var signInManager = sp.GetRequiredService<SignInManager<IdentityUser>>();
                var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: true, lockoutOnFailure: true);
                return result.Succeeded ? TypedResults.Ok() : TypedResults.Unauthorized();
            })(loginRequest, serviceProvider);

            // Assert
            Assert.IsType<Ok>(result);
        }

        [Fact]
        public async Task MapIdentityApi_WithInvalidLoginRequest_ReturnsUnauthorized()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "wrongpassword" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>().MapPost("/login", async (LoginRequest login, [FromServices] IServiceProvider sp) =>
            {
                var signInManager = sp.GetRequiredService<SignInManager<IdentityUser>>();
                var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: true, lockoutOnFailure: true);
                return result.Succeeded ? TypedResults.Ok() : TypedResults.Unauthorized();
            })(loginRequest, serviceProvider);

            // Assert
            Assert.IsType<Unauthorized>(result);
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
    }

    public class IdentityDbContext : IdentityDbContext<IdentityUser, IdentityRole>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }
    }
}
