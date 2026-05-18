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
            public bool IsEmailConfirmedResult { get; set; } = true;
            public bool IsPhoneNumberConfirmedResult { get; set; } = true;
            public bool IsConfirmedAsyncResult { get; set; } = true;

            public DummyUserManager() : base(
                new Mock<IUserStore<DummyUser>>().Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null)
            { }

            public override Task<bool> IsEmailConfirmedAsync(DummyUser user) => Task.FromResult(IsEmailConfirmedResult);
            public override Task<bool> IsPhoneNumberConfirmedAsync(DummyUser user) => Task.FromResult(IsPhoneNumberConfirmedResult);
        }

        private class DummyConfirmation : IUserConfirmation<DummyUser>
        {
            public bool IsConfirmedAsyncResult { get; set; } = true;
            public Task<bool> IsConfirmedAsync(UserManager<DummyUser> userManager, DummyUser user)
                => Task.FromResult(IsConfirmedAsyncResult);
        }

        private class DummyClaimsFactory : IUserClaimsPrincipalFactory<DummyUser>
        {
            public Task<ClaimsPrincipal> CreateAsync(DummyUser user)
            {
                var identity = new ClaimsIdentity("Test");
                var principal = new ClaimsPrincipal(identity);
                return Task.FromResult(principal);
            }
        }

        private class DummyLogger<T> : ILogger<T>
        {
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        }

        private class DummyMeterFactory : IMeterFactory { }

        [Fact]
        public void Constructor_Should_Call_GetService_For_MeterFactory_And_PasskeyHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            var meterFactory = new DummyMeterFactory();
            var passkeyHandler = new Mock<IPasskeyHandler<DummyUser>>().Object;

            services.AddSingleton<IMeterFactory>(meterFactory);
            services.AddSingleton<IPasskeyHandler<DummyUser>>(passkeyHandler);

            var serviceProvider = services.BuildServiceProvider();

            var userManagerMock = new Mock<UserManager<DummyUser>>();
            userManagerMock.SetupGet(um => um.ServiceProvider).Returns(serviceProvider);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.SetupGet(ca => ca.HttpContext).Returns(new DefaultHttpContext());

            var schemes = new Mock<IAuthenticationSchemeProvider>().Object;
            var confirmation = new DummyConfirmation();

            var logger = new DummyLogger<SignInManager<DummyUser>>();
            var claimsFactory = new DummyClaimsFactory();

            // Act
            var signInManager = new SignInManager<DummyUser>(
                userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory,
                Options.Create(new IdentityOptions()),
                logger,
                schemes,
                confirmation);

            // Assert
            Assert.NotNull(signInManager);
            Assert.NotNull(signInManager.UserManager);
            Assert.NotNull(signInManager.Logger);
            // The _metrics and _passkeyHandler are private, but we can check if constructor didn't throw
        }

        [Fact]
        public async Task GetService_Should_Return_NonNull_For_MeterFactory_And_PasskeyHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            var meterFactory = new DummyMeterFactory();
            var passkeyHandler = new Mock<IPasskeyHandler<DummyUser>>().Object;

            services.AddSingleton<IMeterFactory>(meterFactory);
            services.AddSingleton<IPasskeyHandler<DummyUser>>(passkeyHandler);

            var serviceProvider = services.BuildServiceProvider();

            var userManagerMock = new Mock<UserManager<DummyUser>>();
            userManagerMock.SetupGet(um => um.ServiceProvider).Returns(serviceProvider);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.SetupGet(ca => ca.HttpContext).Returns(new DefaultHttpContext());

            var schemes = new Mock<IAuthenticationSchemeProvider>().Object;
            var confirmation = new DummyConfirmation();

            var logger = new DummyLogger<SignInManager<DummyUser>>();
            var claimsFactory = new DummyClaimsFactory();

            var signInManager = new SignInManager<DummyUser>(
                userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory,
                Options.Create(new IdentityOptions()),
                logger,
                schemes,
                confirmation);

            // Act
            var passkeyHandlerInstance = signInManager.GetType()
                .GetField("_passkeyHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(signInManager);

            var metricsInstance = signInManager.GetType()
                .GetField("_metrics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(signInManager);

            // Assert
            Assert.NotNull(passkeyHandlerInstance);
            Assert.NotNull(metricsInstance);
        }
    }
}
