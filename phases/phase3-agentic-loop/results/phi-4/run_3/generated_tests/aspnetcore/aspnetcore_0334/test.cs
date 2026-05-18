using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class SignInManagerTests
{
    [Fact]
    public void Constructor_SetsPasskeyHandler_WhenServiceProviderReturnsHandler()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<IdentityUser>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var passkeyHandlerMock = new Mock<IPasskeyHandler<IdentityUser>>();

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)))
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

        // Use reflection to access the private field
        var passkeyHandlerField = typeof(SignInManager<IdentityUser>).GetField("_passkeyHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        var passkeyHandler = passkeyHandlerField.GetValue(signInManager);

        // Assert
        Assert.Same(passkeyHandlerMock.Object, passkeyHandler);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)), Times.Once);
    }
}
