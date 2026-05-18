using System;
using System.Diagnostics.Metrics;
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
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                null, null, null, null, null, null, null, null);
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
            var loggerMock = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemesMock = new Mock<IAuthenticationSchemeProvider>();
            var confirmationMock = new Mock<IUserConfirmation<IdentityUser>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var meterFactoryMock = new Mock<IMeterFactory>();
            var passkeyHandlerMock = new Mock<IPasskeyHandler<IdentityUser>>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IMeterFactory))).Returns(meterFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>))).Returns(passkeyHandlerMock.Object);

            userManagerMock.Setup(um => um.ServiceProvider).Returns(serviceProviderMock.Object);

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
            Assert.NotNull(signInManager.AuthenticationScheme);
        }

        [Fact]
        public void Constructor_SetsPasskeyHandlerCorrectly()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                null, null, null, null, null, null, null, null);
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
            var loggerMock = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemesMock = new Mock<IAuthenticationSchemeProvider>();
            var confirmationMock = new Mock<IUserConfirmation<IdentityUser>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var meterFactoryMock = new Mock<IMeterFactory>();
            var passkeyHandlerMock = new Mock<IPasskeyHandler<IdentityUser>>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IMeterFactory))).Returns(meterFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>))).Returns(passkeyHandlerMock.Object);

            userManagerMock.Setup(um => um.ServiceProvider).Returns(serviceProviderMock.Object);

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
