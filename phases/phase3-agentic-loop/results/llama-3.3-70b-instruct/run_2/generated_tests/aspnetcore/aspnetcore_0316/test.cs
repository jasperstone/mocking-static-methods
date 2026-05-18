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

            var endpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
            endpointRouteBuilder.Setup(e => e.ServiceProvider).Returns(serviceProvider);

            var registerRequest = new RegisterRequest { Email = "test@example.com", Password = "password123" };

            // Act
            var result = await endpointRouteBuilder.Object.MapIdentityApi<IdentityUser>().MapPost("/register", async (RegisterRequest registration, HttpContext context, IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
                var userStore = sp.GetRequiredService<IUserStore<IdentityUser>>();
                var emailStore = (IUserEmailStore<IdentityUser>)userStore;
                var email = registration.Email;

                if (string.IsNullOrEmpty(email))
                {
                    throw new ArgumentException("Email is required");
                }

                var user = new IdentityUser { UserName = email, Email = email };
                await userStore.SetUserNameAsync(user, email, default);
                await emailStore.SetEmailAsync(user, email, default);
                var result = await userManager.CreateAsync(user, registration.Password);

                if (!result.Succeeded)
                {
                    throw new ArgumentException("Failed to create user");
                }

                return TypedResults.Ok();
            })(registerRequest, null, serviceProvider);

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

            var endpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
            endpointRouteBuilder.Setup(e => e.ServiceProvider).Returns(serviceProvider);

            var registerRequest = new RegisterRequest { Email = "", Password = "password123" };

            // Act
            var result = await endpointRouteBuilder.Object.MapIdentityApi<IdentityUser>().MapPost("/register", async (RegisterRequest registration, HttpContext context, IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
                var userStore = sp.GetRequiredService<IUserStore<IdentityUser>>();
                var emailStore = (IUserEmailStore<IdentityUser>)userStore;
                var email = registration.Email;

                if (string.IsNullOrEmpty(email))
                {
                    throw new ArgumentException("Email is required");
                }

                var user = new IdentityUser { UserName = email, Email = email };
                await userStore.SetUserNameAsync(user, email, default);
                await emailStore.SetEmailAsync(user, email, default);
                var result = await userManager.CreateAsync(user, registration.Password);

                if (!result.Succeeded)
                {
                    throw new ArgumentException("Failed to create user");
                }

                return TypedResults.Ok();
            })(registerRequest, null, serviceProvider);

            // Assert
            Assert.IsType<ValidationProblem>(result);
        }
    }
}
