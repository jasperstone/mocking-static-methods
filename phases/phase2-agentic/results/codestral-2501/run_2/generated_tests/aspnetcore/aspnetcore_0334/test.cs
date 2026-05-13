using System;
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
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
            var loggerMock = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemesMock = new Mock<IAuthenticationSchemeProvider>();
            var confirmationMock = new Mock<IUserConfirmation<IdentityUser>>();

            // Act
            var signInManager = new SignInManager<IdentityUser>(
                userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                optionsAccessorMock.Object,
                loggerMock.Object,
                schemesMock.Object,
                confirmationMock.Object);

            // Assert
            Assert.NotNull(signInManager.UserManager);
            Assert.NotNull(signInManager.ClaimsFactory);
            Assert.NotNull(signInManager.Options);
            Assert.NotNull(signInManager.Logger);
        }

        [Fact]
        public void Constructor_GetServiceCalledOnServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                serviceProviderMock.Object,
                new Mock<ILogger<UserManager<IdentityUser>>>().Object);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
            var loggerMock = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemesMock = new Mock<IAuthenticationSchemeProvider>();
            var confirmationMock = new Mock<IUserConfirmation<IdentityUser>>();

            // Act
            var signInManager = new SignInManager<IdentityUser>(
                userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                optionsAccessorMock.Object,
                loggerMock.Object,
                schemesMock.Object,
                confirmationMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)), Times.Once);
        }
    }
}
