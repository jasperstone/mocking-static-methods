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
        public void Constructor_Should_Call_GetService_For_MeterFactory_And_PasskeyHandler()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var meterFactoryMock = new Mock<IMeterFactory>();
            var passkeyHandlerMock = new Mock<IPasskeyHandler<DummyUser>>();

            serviceProviderMock.Setup(sp => sp.GetService<IMeterFactory>()).Returns(meterFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService<IPasskeyHandler<DummyUser>>()).Returns(passkeyHandlerMock.Object);

            var userManager = new DummyUserManager();
            var contextAccessor = new Mock<IHttpContextAccessor>();
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
            // Verify that _metrics and _passkeyHandler are set
            var passkeyHandlerField = typeof(SignInManager<DummyUser>).GetField("_passkeyHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var metricsField = typeof(SignInManager<DummyUser>).GetField("_metrics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(passkeyHandlerField.GetValue(signInManager));
            Assert.NotNull(metricsField.GetValue(signInManager));
        }
    }
}
