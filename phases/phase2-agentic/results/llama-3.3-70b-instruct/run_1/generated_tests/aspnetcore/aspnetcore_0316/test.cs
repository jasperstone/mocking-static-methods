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
        public async Task MapIdentityApi_RegisterEndpoint_ValidRegistration_ReturnsOkResult()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var registerRequest = new RegisterRequest { Email = "test@example.com", Password = "password123" };

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
            })(registerRequest, new DefaultHttpContext(), serviceProvider);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task MapIdentityApi_RegisterEndpoint_InvalidRegistration_ReturnsValidationProblem()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var registerRequest = new RegisterRequest { Email = "", Password = "password123" };

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
            })(registerRequest, new DefaultHttpContext(), serviceProvider);

            // Assert
            Assert.IsType<ValidationProblemResult>(result);
        }

        [Fact]
        public async Task MapIdentityApi_LoginEndpoint_ValidLogin_ReturnsOkResult()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "password123" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>().MapPost("/login", async (LoginRequest login, IServiceProvider sp) =>
            {
                var signInManager = sp.GetRequiredService<SignInManager<IdentityUser>>();
                var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, true, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    return Results.Problem(result.ToString(), statusCode: 401);
                }

                return Results.Ok();
            })(loginRequest, serviceProvider);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task MapIdentityApi_LoginEndpoint_InvalidLogin_ReturnsProblemResult()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "wrongpassword" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>().MapPost("/login", async (LoginRequest login, IServiceProvider sp) =>
            {
                var signInManager = sp.GetRequiredService<SignInManager<IdentityUser>>();
                var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, true, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    return Results.Problem(result.ToString(), statusCode: 401);
                }

                return Results.Ok();
            })(loginRequest, serviceProvider);

            // Assert
            Assert.IsType<ProblemResult>(result);
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
