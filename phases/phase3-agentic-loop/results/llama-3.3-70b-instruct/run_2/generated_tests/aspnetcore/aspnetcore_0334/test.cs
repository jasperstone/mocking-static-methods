using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class SignInManagerTests
    {
        [Fact]
        public async Task Constructor_GetService_IPasskeyHandler()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IPasskeyHandler<IdentityUser>>(provider => new PasskeyHandler<IdentityUser>(new UserManager<IdentityUser>(new Mock<IUserStore<IdentityUser>>().Object, new Mock<IOptions<IdentityOptions>>().Object, new Mock<IPasswordHasher<IdentityUser>>().Object, new Mock<IEnumerable<IUserValidator<IdentityUser>>>().Object, new Mock<IEnumerable<IPasswordValidator<IdentityUser>>>().Object, new Mock<ILookupNormalizer>().Object, new Mock<IdentityErrorDescriber>().Object, serviceProvider, new Mock<ILogger<UserManager<IdentityUser>>>().Object), new Mock<IOptions<IdentityPasskeyOptions>>().Object))
                .BuildServiceProvider();

            var userManager = new UserManager<IdentityUser>(new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new Mock<IEnumerable<IUserValidator<IdentityUser>>>().Object,
                new Mock<IEnumerable<IPasswordValidator<IdentityUser>>>().Object,
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                serviceProvider,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object);

            // Act
            var signInManager = new SignInManager<IdentityUser>(userManager,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<IdentityUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<IdentityUser>>().Object);

            // Assert
            Assert.NotNull(signInManager._passkeyHandler);
        }
    }
}
