using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class SignInManagerTests
    {
        [Fact]
        public void Constructor_WithNullUserManager_ThrowsArgumentNullException()
        {
            // Arrange
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<IdentityUser>>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SignInManager<IdentityUser>(
                null,
                contextAccessor.Object,
                claimsFactory.Object,
                optionsAccessor.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object));
        }

        [Fact]
        public void Constructor_WithNullContextAccessor_ThrowsArgumentNullException()
        {
            // Arrange
            var userManager = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object);
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<IdentityUser>>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SignInManager<IdentityUser>(
                userManager.Object,
                null,
                claimsFactory.Object,
                optionsAccessor.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object));
        }

        [Fact]
        public void Constructor_WithNullClaimsFactory_ThrowsArgumentNullException()
        {
            // Arrange
            var userManager = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object);
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<IdentityUser>>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SignInManager<IdentityUser>(
                userManager.Object,
                contextAccessor.Object,
                null,
                optionsAccessor.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object));
        }

        [Fact]
        public void Constructor_WithServiceProvider_InitializesPasskeyHandler()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var passkeyHandlerMock = new Mock<IPasskeyHandler<IdentityUser>>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>))).Returns(passkeyHandlerMock.Object);

            var userManager = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                serviceProviderMock.Object,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object);
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<IdentityUser>>();

            // Act
            var signInManager = new SignInManager<IdentityUser>(
                userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                optionsAccessor.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object);

            // Assert
            Assert.NotNull(signInManager);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)), Times.Once);
        }
    }
}
