using System;
using System.Reflection;
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
            .SetupGet(u => u.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        // Act
        var signInManager = new SignInManager<IdentityUser>(
            userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<IdentityUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<IdentityUser>>());

        // Assert
        var passkeyHandlerField = typeof(SignInManager<IdentityUser>).GetField("_passkeyHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(passkeyHandlerField);
        var actualPasskeyHandler = passkeyHandlerField.GetValue(signInManager);
        Assert.Same(passkeyHandlerMock.Object, actualPasskeyHandler);
    }

    [Fact]
    public void Constructor_DoesNotSetPasskeyHandler_WhenServiceProviderReturnsNull()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<IdentityUser>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)))
            .Returns((IPasskeyHandler<IdentityUser>)null);

        userManagerMock
            .SetupGet(u => u.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        // Act
        var signInManager = new SignInManager<IdentityUser>(
            userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<IdentityUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<IdentityUser>>());

        // Assert
        var passkeyHandlerField = typeof(SignInManager<IdentityUser>).GetField("_passkeyHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(passkeyHandlerField);
        var actualPasskeyHandler = passkeyHandlerField.GetValue(signInManager);
        Assert.Null(actualPasskeyHandler);
    }
}
