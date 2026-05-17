using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IdentityTests
{
    public class SignInManagerTests
    {
        [Fact]
        public async Task CanSignInAsync_ReturnsTrue_WhenUserIsConfirmed()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentityCore<IdentityUser>(options => options.SignIn.RequireConfirmedEmail = true)
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            var signInManager = new SignInManager<IdentityUser>(userManager, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(), Mock.Of<IOptions<IdentityOptions>>(), Mock.Of<ILogger<SignInManager<IdentityUser>>>(), Mock.Of<IAuthenticationSchemeProvider>(), Mock.Of<IUserConfirmation<IdentityUser>>());

            var user = new IdentityUser { Email = "test@example.com" };
            await userManager.CreateAsync(user);
            await userManager.ConfirmEmailAsync(user, "confirmation-token");

            // Act
            var result = await signInManager.CanSignInAsync(user);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CanSignInAsync_ReturnsFalse_WhenUserIsNotConfirmed()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddIdentityCore<IdentityUser>(options => options.SignIn.RequireConfirmedEmail = true)
                .AddEntityFrameworkStores<IdentityDbContext>()
                .BuildServiceProvider();

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            var signInManager = new SignInManager<IdentityUser>(userManager, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(), Mock.Of<IOptions<IdentityOptions>>(), Mock.Of<ILogger<SignInManager<IdentityUser>>>(), Mock.Of<IAuthenticationSchemeProvider>(), Mock.Of<IUserConfirmation<IdentityUser>>());

            var user = new IdentityUser { Email = "test@example.com" };
            await userManager.CreateAsync(user);

            // Act
            var result = await signInManager.CanSignInAsync(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetService_ReturnsService_WhenServiceIsRegistered()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPasskeyHandler<IdentityUser>, PasskeyHandler<IdentityUser>>()
                .BuildServiceProvider();

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            var signInManager = new SignInManager<IdentityUser>(userManager, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(), Mock.Of<IOptions<IdentityOptions>>(), Mock.Of<ILogger<SignInManager<IdentityUser>>>(), Mock.Of<IAuthenticationSchemeProvider>(), Mock.Of<IUserConfirmation<IdentityUser>>());

            // Act
            var service = signInManager._passkeyHandler;

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void GetService_ReturnsNull_WhenServiceIsNotRegistered()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            var signInManager = new SignInManager<IdentityUser>(userManager, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(), Mock.Of<IOptions<IdentityOptions>>(), Mock.Of<ILogger<SignInManager<IdentityUser>>>(), Mock.Of<IAuthenticationSchemeProvider>(), Mock.Of<IUserConfirmation<IdentityUser>>());

            // Act
            var service = signInManager._passkeyHandler;

            // Assert
            Assert.Null(service);
        }
    }

    public class PasskeyHandler<TUser> : IPasskeyHandler<TUser> where TUser : class
    {
        public Task<bool> IsValidAsync(TUser user, string passkey)
        {
            throw new NotImplementedException();
        }
    }

    public class IdentityDbContext : IdentityDbContext<IdentityUser>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }
    }
}
