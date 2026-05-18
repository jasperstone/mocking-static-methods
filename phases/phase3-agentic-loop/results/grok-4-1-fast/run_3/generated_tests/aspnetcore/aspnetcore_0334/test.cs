using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests;

public class SignInManagerTests
{
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IPasskeyHandler<IdentityUser>> _mockPasskeyHandler;

    public SignInManagerTests()
    {
        _mockPasskeyHandler = new Mock<IPasskeyHandler<IdentityUser>>();
        
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>))).Returns(_mockPasskeyHandler.Object);

        var mockUserStore = new Mock<IUserStore<IdentityUser>>();
        var mockPasswordHasher = Mock.Of<IPasswordHasher<IdentityUser>>();
        var mockNormalizer = Mock.Of<ILookupNormalizer>();
        
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            mockUserStore.Object,
            Options.Create(new IdentityOptions()),
            mockPasswordHasher,
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            mockNormalizer,
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            NullLogger<UserManager<IdentityUser>>.Instance);

        _mockUserManager.SetupGet(um => um.ServiceProvider).Returns(_mockServiceProvider.Object);
    }

    [Fact]
    public void Constructor_CallsGetService_OnUserManagerServiceProvider()
    {
        // Arrange
        var mockContextAccessor = new Mock<IHttpContextAccessor>();
        var mockClaimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        var mockSchemes = new Mock<IAuthenticationSchemeProvider>();
        var mockConfirmation = new Mock<IUserConfirmation<IdentityUser>>();

        // Act
        _ = new SignInManager<IdentityUser>(
            _mockUserManager.Object,
            mockContextAccessor.Object,
            mockClaimsFactory.Object,
            Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<IdentityUser>>.Instance,
            mockSchemes.Object,
            mockConfirmation.Object);

        // Assert
        _mockServiceProvider.Verify(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)), Times.Once());
        _mockUserManager.VerifyGet(um => um.ServiceProvider, Times.AtLeastOnce());
    }

    [Fact]
    public void Constructor_UserManagerServiceProviderNull_DoesNotThrow()
    {
        // Arrange
        _mockUserManager.SetupGet(um => um.ServiceProvider).Returns((IServiceProvider?)null);
        var mockContextAccessor = new Mock<IHttpContextAccessor>();
        var mockClaimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        var mockSchemes = new Mock<IAuthenticationSchemeProvider>();
        var mockConfirmation = new Mock<IUserConfirmation<IdentityUser>>();

        // Act
        var ex = Record.Exception(() => new SignInManager<IdentityUser>(
            _mockUserManager.Object,
            mockContextAccessor.Object,
            mockClaimsFactory.Object,
            Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<IdentityUser>>.Instance,
            mockSchemes.Object,
            mockConfirmation.Object));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void Constructor_ServiceProviderReturnsNullPasskeyHandler_DoesNotThrow()
    {
        // Arrange
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>))).Returns((IPasskeyHandler<IdentityUser>?)null);
        var mockContextAccessor = new Mock<IHttpContextAccessor>();
        var mockClaimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        var mockSchemes = new Mock<IAuthenticationSchemeProvider>();
        var mockConfirmation = new Mock<IUserConfirmation<IdentityUser>>();

        // Act
        var ex = Record.Exception(() => new SignInManager<IdentityUser>(
            _mockUserManager.Object,
            mockContextAccessor.Object,
            mockClaimsFactory.Object,
            Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<IdentityUser>>.Instance,
            mockSchemes.Object,
            mockConfirmation.Object));

        // Assert
        Assert.Null(ex);
        _mockServiceProvider.Verify(sp => sp.GetService(typeof(IPasskeyHandler<IdentityUser>)), Times.Once());
    }
}
