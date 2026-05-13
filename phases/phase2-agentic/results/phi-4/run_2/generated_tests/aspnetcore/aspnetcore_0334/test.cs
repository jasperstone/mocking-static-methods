using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SignInManagerTests
{
    [Fact]
    public void Constructor_Sets_Metrics_And_PasskeyHandler_Correctly()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<IdentityUser>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var meterFactoryMock = new Mock<IMeterFactory>();
        var passkeyHandlerMock = new Mock<IPasskeyHandler<IdentityUser>>();

        serviceProviderMock
            .Setup(sp => sp.GetService<IMeterFactory>())
            .Returns(meterFactoryMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService<IPasskeyHandler<IdentityUser>>())
            .Returns(passkeyHandlerMock.Object);

        userManagerMock
            .SetupGet(u => u.ServiceProvider)
            .Returns(serviceProviderMock.Object);

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
        Assert.NotNull(signInManager._metrics);
        meterFactoryMock.Verify(v => v.CreateCounter(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        Assert.Same(passkeyHandlerMock.Object, signInManager._passkeyHandler);
    }
}
