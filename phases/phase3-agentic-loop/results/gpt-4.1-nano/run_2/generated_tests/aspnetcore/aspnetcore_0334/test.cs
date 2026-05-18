using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Identity.Tests
{
    public class SignInManagerTests
    {
        private class DummyUser { }

        private class DummyUserManager : UserManager<DummyUser>
        {
            public DummyUserManager() : base(
                Mock.Of<IUserStore<DummyUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<DummyUser>>(),
                Array.Empty<IUserValidator<DummyUser>>(),
                Array.Empty<IPasswordValidator<DummyUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                null,
                null)
            { }

            public override Task<bool> IsEmailConfirmedAsync(DummyUser user) => Task.FromResult(true);
            public override Task<bool> IsPhoneNumberConfirmedAsync(DummyUser user) => Task.FromResult(true);
        }

        [Fact]
        public void Constructor_Should_Call_GetService_For_IMeterFactory_And_IPasskeyHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            var meterFactoryMock = new Mock<IMeterFactory>();
            var passkeyHandlerMock = new Mock<IPasskeyHandler<DummyUser>>();

            services.AddSingleton<IMeterFactory>(meterFactoryMock.Object);
            services.AddSingleton<IPasskeyHandler<DummyUser>>(passkeyHandlerMock.Object);
            services.AddTransient<UserManager<DummyUser>, DummyUserManager>();
            var serviceProvider = services.BuildServiceProvider();

            var userManager = serviceProvider.GetRequiredService<UserManager<DummyUser>>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(c => c.HttpContext).Returns(new DefaultHttpContext());
            var claimsFactory = Mock.Of<IUserClaimsPrincipalFactory<DummyUser>>();
            var optionsAccessor = Options.Create(new IdentityOptions());
            var logger = Mock.Of<ILogger<SignInManager<DummyUser>>>();
            var schemes = Mock.Of<IAuthenticationSchemeProvider>();
            var confirmation = Mock.Of<IUserConfirmation<DummyUser>>();

            // Act
            var signInManager = new SignInManager<DummyUser>(
                userManager,
                contextAccessor.Object,
                claimsFactory,
                optionsAccessor,
                logger,
                schemes,
                confirmation);

            // Assert
            Assert.NotNull(signInManager);
            Assert.NotNull(signInManager.UserManager);
            Assert.NotNull(signInManager.Logger);
            // Verify that _passkeyHandler is set
            var passkeyHandlerField = typeof(SignInManager<DummyUser>).GetField("_passkeyHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var passkeyHandlerValue = passkeyHandlerField.GetValue(signInManager);
            Assert.NotNull(passkeyHandlerValue);
            Assert.IsType<IPasskeyHandler<DummyUser>>(passkeyHandlerValue);
        }

        [Fact]
        public void Context_Property_Should_Throw_When_HttpContext_Is_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<UserManager<DummyUser>, DummyUserManager>();
            var provider = services.BuildServiceProvider();

            var userManager = provider.GetRequiredService<UserManager<DummyUser>>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(c => c.HttpContext).Returns((HttpContext)null);
            var signInManager = new SignInManager<DummyUser>(
                userManager,
                contextAccessor.Object,
                Mock.Of<IUserClaimsPrincipalFactory<DummyUser>>(),
                Options.Create(new IdentityOptions()),
                Mock.Of<ILogger<SignInManager<DummyUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<DummyUser>>());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => { var ctx = signInManager.Context; });
        }

        [Fact]
        public async Task IsSignedIn_Should_Return_True_For_Authenticated_User()
        {
            // Arrange
            var userManager = new DummyUserManager();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Authentication, "TestScheme") };
            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            context.User = principal;
            contextAccessor.Setup(c => c.HttpContext).Returns(context);

            var signInManager = new SignInManager<DummyUser>(
                userManager,
                contextAccessor.Object,
                Mock.Of<IUserClaimsPrincipalFactory<DummyUser>>(),
                Options.Create(new IdentityOptions()),
                Mock.Of<ILogger<SignInManager<DummyUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<DummyUser>>());

            // Act
            var result = signInManager.IsSignedIn(principal);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CanSignInAsync_Should_Return_False_If_Email_Not_Confirmed()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<DummyUser>>();
            userManagerMock.Setup(um => um.IsEmailConfirmedAsync(It.IsAny<DummyUser>())).ReturnsAsync(false);
            var options = new IdentityOptions { SignIn = { RequireConfirmedEmail = true } };
            var confirmationMock = new Mock<IUserConfirmation<DummyUser>>();
            var signInManager = new SignInManager<DummyUser>(
                userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                Mock.Of<IUserClaimsPrincipalFactory<DummyUser>>(),
                Options.Create(options),
                Mock.Of<ILogger<SignInManager<DummyUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                confirmationMock.Object);

            // Act
            var canSignIn = await signInManager.CanSignInAsync(new DummyUser());

            // Assert
            Assert.False(canSignIn);
        }

        [Fact]
        public async Task CanSignInAsync_Should_Return_False_If_Phone_Not_Confirmed()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<DummyUser>>();
            userManagerMock.Setup(um => um.IsEmailConfirmedAsync(It.IsAny<DummyUser>())).ReturnsAsync(true);
            userManagerMock.Setup(um => um.IsPhoneNumberConfirmedAsync(It.IsAny<DummyUser>())).ReturnsAsync(false);
            var options = new IdentityOptions { SignIn = { RequireConfirmedPhoneNumber = true } };
            var confirmationMock = new Mock<IUserConfirmation<DummyUser>>();
            var signInManager = new SignInManager<DummyUser>(
                userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                Mock.Of<IUserClaimsPrincipalFactory<DummyUser>>(),
                Options.Create(options),
                Mock.Of<ILogger<SignInManager<DummyUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                confirmationMock.Object);

            // Act
            var canSignIn = await signInManager.CanSignInAsync(new DummyUser());

            // Assert
            Assert.False(canSignIn);
        }

        [Fact]
        public async Task CanSignInAsync_Should_Return_False_If_Account_Not_Confirmed()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<DummyUser>>();
            userManagerMock.Setup(um => um.IsEmailConfirmedAsync(It.IsAny<DummyUser>())).ReturnsAsync(true);
            userManagerMock.Setup(um => um.IsPhoneNumberConfirmedAsync(It.IsAny<DummyUser>())).ReturnsAsync(true);
            var confirmationMock = new Mock<IUserConfirmation<DummyUser>>();
            confirmationMock.Setup(c => c.IsConfirmedAsync(It.IsAny<UserManager<DummyUser>>(), It.IsAny<DummyUser>())).ReturnsAsync(false);
            var options = new IdentityOptions { SignIn = { RequireConfirmedAccount = true } };
            var signInManager = new SignInManager<DummyUser>(
                userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                Mock.Of<IUserClaimsPrincipalFactory<DummyUser>>(),
                Options.Create(options),
                Mock.Of<ILogger<SignInManager<DummyUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                confirmationMock.Object);

            // Act
            var canSignIn = await signInManager.CanSignInAsync(new DummyUser());

            // Assert
            Assert.False(canSignIn);
        }
    }
}
