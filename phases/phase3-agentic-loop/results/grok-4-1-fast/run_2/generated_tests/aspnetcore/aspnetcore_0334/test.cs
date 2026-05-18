using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class SignInManagerTests
    {
        [Fact]
        public void Constructor_WithServiceProvider_CallsGetServiceForPasskeyHandler()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)))
                              .Returns((IPasskeyHandler<IdentityUser>?)null);

            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                Array.Empty<IUserValidator<IdentityUser>>(),
                Array.Empty<IPasswordValidator<IdentityUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                serviceProviderMock.Object,
                Mock.Of<ILogger<UserManager<IdentityUser>>>());
            userManagerMock.SetupGet(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            optionsMock.SetupGet(o => o.Value).Returns(new IdentityOptions());
            var schemesMock = new Mock<IAuthenticationSchemeProvider>();
            var confirmationMock = new Mock<IUserConfirmation<IdentityUser>>();

            // Act
            var exception = Record.Exception(() => new SignInManager<IdentityUser>(
                userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                optionsMock.Object,
                NullLogger<SignInManager<IdentityUser>>.Instance,
                schemesMock.Object,
                confirmationMock.Object));

            // Assert
            Assert.Null(exception);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)), Times.Once());
        }

        [Fact]
        public void Constructor_WithNullServiceProvider_DoesNotThrow()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                Array.Empty<IUserValidator<IdentityUser>>(),
                Array.Empty<IPasswordValidator<IdentityUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                (IServiceProvider)null!,
                Mock.Of<ILogger<UserManager<IdentityUser>>>());
            userManagerMock.SetupGet(u => u.ServiceProvider).Returns((IServiceProvider?)null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            optionsMock.SetupGet(o => o.Value).Returns(new IdentityOptions());
            var schemesMock = new Mock<IAuthenticationSchemeProvider>();
            var confirmationMock = new Mock<IUserConfirmation<IdentityUser>>();

            // Act
            var exception = Record.Exception(() => new SignInManager<IdentityUser>(
                userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                optionsMock.Object,
                NullLogger<SignInManager<IdentityUser>>.Instance,
                schemesMock.Object,
                confirmationMock.Object));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_WithPasskeyHandlerReturned_CallsGetService()
        {
            // Arrange
            var passkeyHandlerMock = new Mock<IPasskeyHandler<IdentityUser>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)))
                              .Returns(passkeyHandlerMock.Object);

            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                Array.Empty<IUserValidator<IdentityUser>>(),
                Array.Empty<IPasswordValidator<IdentityUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                serviceProviderMock.Object,
                Mock.Of<ILogger<UserManager<IdentityUser>>>());
            userManagerMock.SetupGet(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            optionsMock.SetupGet(o => o.Value).Returns(new IdentityOptions());
            var schemesMock = new Mock<IAuthenticationSchemeProvider>();
            var confirmationMock = new Mock<IUserConfirmation<IdentityUser>>();

            // Act
            var exception = Record.Exception(() => new SignInManager<IdentityUser>(
                userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                optionsMock.Object,
                NullLogger<SignInManager<IdentityUser>>.Instance,
                schemesMock.Object,
                confirmationMock.Object));

            // Assert
            Assert.Null(exception);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)), Times.Once());
        }
    }
}
