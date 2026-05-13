using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class SignInManagerTests
{
    [Fact]
    public void Constructor_SetsPasskeyHandler_WhenServiceProviderReturnsService()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<IdentityUser>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var passkeyHandlerMock = new Mock<IPasskeyHandler<IdentityUser>>();

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)))
            .Returns(passkeyHandlerMock.Object);

        userManagerMock
            .SetupGet(um => um.ServiceProvider)
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
        Assert.Same(passkeyHandlerMock.Object, signInManager._passkeyHandler);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)), Times.Once);
    }
}
