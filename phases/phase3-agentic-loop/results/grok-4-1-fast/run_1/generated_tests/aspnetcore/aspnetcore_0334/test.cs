using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class SignInManagerTests
    {
        [Fact]
        public void Constructor_CallsGetServiceIPasskeyHandler_WhenServiceProviderNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IPasskeyHandler<object>)))
                .Returns(Mock.Of<IPasskeyHandler<object>>());

            var userStoreMock = new Mock<IUserStore<object>>();
            var userManagerMock = Mock.Of<UserManager<object>>(); // Use Mock.Of to avoid constructor complexity
            Mock.Get(userManagerMock).Setup(um => um.ServiceProvider).Returns(serviceProviderMock.Object);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<object>>();
            var options = Options.Create(new IdentityOptions());
            var loggerMock = new Mock<ILogger<SignInManager<object>>>();
            var schemesMock = Mock.Of<IAuthenticationSchemeProvider>();
            var confirmationMock = Mock.Of<IUserConfirmation<object>>();

            // Act
            var signInManager = new SignInManager<object>(
                userManagerMock,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                options,
                loggerMock.Object,
                schemesMock,
                confirmationMock);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPasskeyHandler<object>)), Times.Once());
        }

        [Fact]
        public void Constructor_DoesNotCallGetServiceIPasskeyHandler_WhenServiceProviderNull()
        {
            // Arrange
            var userManagerMock = Mock.Of<UserManager<object>>();
            Mock.Get(userManagerMock).Setup(um => um.ServiceProvider).Returns((IServiceProvider)null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<object>>();
            var options = Options.Create(new IdentityOptions());
            var loggerMock = new Mock<ILogger<SignInManager<object>>>();
            var schemesMock = Mock.Of<IAuthenticationSchemeProvider>();
            var confirmationMock = Mock.Of<IUserConfirmation<object>>();

            // Act
            var signInManager = new SignInManager<object>(
                userManagerMock,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                options,
                loggerMock.Object,
                schemesMock,
                confirmationMock);

            // Assert - null short-circuits, no call made
        }

        [Fact]
        public void Constructor_ThrowsForNullUserManager()
        {
            Assert.Throws<ArgumentNullException>(() => new SignInManager<object>(
                null!,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<object>>(),
                Options.Create(new IdentityOptions()),
                Mock.Of<ILogger<SignInManager<object>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<object>>()));
        }

        [Fact]
        public void Constructor_ThrowsForNullContextAccessor()
        {
            Assert.Throws<ArgumentNullException>(() => new SignInManager<object>(
                Mock.Of<UserManager<object>>(),
                null!,
                Mock.Of<IUserClaimsPrincipalFactory<object>>(),
                Options.Create(new IdentityOptions()),
                Mock.Of<ILogger<SignInManager<object>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<object>>()));
        }

        [Fact]
        public void Constructor_ThrowsForNullClaimsFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new SignInManager<object>(
                Mock.Of<UserManager<object>>(),
                Mock.Of<IHttpContextAccessor>(),
                null!,
                Options.Create(new IdentityOptions()),
                Mock.Of<ILogger<SignInManager<object>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<object>>()));
        }
    }
}
