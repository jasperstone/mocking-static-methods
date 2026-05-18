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
        public async Task MapIdentityApi_RegisterEndpoint_WithValidRegistration_ReturnsOkResult()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var registerRequest = new RegisterRequest { Email = "test@example.com", Password = "password" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>()
                .MapPost("/register", async (RegisterRequest registration, HttpContext context, IServiceProvider sp) =>
                {
                    var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
                    var userStore = sp.GetRequiredService<IUserStore<IdentityUser>>();
                    var emailStore = (IUserEmailStore<IdentityUser>)userStore;
                    var email = registration.Email;

                    if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
                    {
                        return Results.ValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
                    }

                    var user = new IdentityUser();
                    await userStore.SetUserNameAsync(user, email, CancellationToken.None);
                    await emailStore.SetEmailAsync(user, email, CancellationToken.None);
                    var result = await userManager.CreateAsync(user, registration.Password);

                    if (!result.Succeeded)
                    {
                        return Results.ValidationProblem(result);
                    }

                    await SendConfirmationEmailAsync(user, userManager, context, email);
                    return Results.Ok();
                })
                .Build()
                .ExecuteAsync(registerRequest);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task MapIdentityApi_RegisterEndpoint_WithInvalidRegistration_ReturnsValidationProblem()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new EndpointRouteBuilder(serviceProvider);
            var registerRequest = new RegisterRequest { Email = "invalid-email", Password = "password" };

            // Act
            var result = await endpointRouteBuilder.MapIdentityApi<IdentityUser>()
                .MapPost("/register", async (RegisterRequest registration, HttpContext context, IServiceProvider sp) =>
                {
                    var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
                    var userStore = sp.GetRequiredService<IUserStore<IdentityUser>>();
                    var emailStore = (IUserEmailStore<IdentityUser>)userStore;
                    var email = registration.Email;

                    if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
                    {
                        return Results.ValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
                    }

                    var user = new IdentityUser();
                    await userStore.SetUserNameAsync(user, email, CancellationToken.None);
                    await emailStore.SetEmailAsync(user, email, CancellationToken.None);
                    var result = await userManager.CreateAsync(user, registration.Password);

                    if (!result.Succeeded)
                    {
                        return Results.ValidationProblem(result);
                    }

                    await SendConfirmationEmailAsync(user, userManager, context, email);
                    return Results.Ok();
                })
                .Build()
                .ExecuteAsync(registerRequest);

            // Assert
            Assert.IsType<ValidationProblemResult>(result);
        }

        private static async Task SendConfirmationEmailAsync(IdentityUser user, UserManager<IdentityUser> userManager, HttpContext context, string email)
        {
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = user.Id, code = code, returnUrl = "/" },
                protocol: context.Request.Scheme);

            await userManager.SendEmailAsync(user, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        }
    }
}
