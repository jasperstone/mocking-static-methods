using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class SignInManagerTests
{
    [Fact]
    public void Constructor_ShouldRetrievePasskeyHandlerFromServiceProvider()
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

        // Assert
        var passkeyHandlerField = typeof(SignInManager<IdentityUser>).GetField("_passkeyHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(passkeyHandlerField);
        var retrievedPasskeyHandler = passkeyHandlerField.GetValue(signInManager);
        Assert.Same(passkeyHandlerMock.Object, retrievedPasskeyHandler);
    }
}
