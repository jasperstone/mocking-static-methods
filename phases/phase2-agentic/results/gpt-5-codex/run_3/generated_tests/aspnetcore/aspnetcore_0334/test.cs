using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Telemetry;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class SignInManagerTests
    {
        [Fact]
        public void Constructor_ResolvesPasskeyHandler_FromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var expectedHandler = new TestPasskeyHandler();
            serviceCollection.AddSingleton<IPasskeyHandler<TestUser>>(expectedHandler);
            serviceCollection.AddSingleton<IMeterFactory, NoopMeterFactory>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var userManager = TestUserManager.Create(serviceProvider);

            // Act
            var signInManager = CreateSignInManager(userManager);

            // Assert
            Assert.Same(expectedHandler, signInManager.GetPasskeyHandler());
        }

        [Fact]
        public void Constructor_SetsPasskeyHandlerToNull_WhenServiceNotRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var userManager = TestUserManager.Create(serviceProvider);

            // Act
            var signInManager = CreateSignInManager(userManager);

            // Assert
            Assert.Null(signInManager.GetPasskeyHandler());
        }

        private static TestSignInManager CreateSignInManager(UserManager<TestUser> userManager)
        {
            return new TestSignInManager(
                userManager,
                new HttpContextAccessor(),
                new TestUserClaimsPrincipalFactory(),
                Options.Create(new IdentityOptions()),
                new LoggerFactory().CreateLogger<SignInManager<TestUser>>(),
                new TestAuthenticationSchemeProvider(),
                new TestUserConfirmation());
        }

        private class TestUser : IdentityUser { }

        private class TestPasskeyHandler : IPasskeyHandler<TestUser>
        {
            public Task<PasskeySignInResult> SignInAsync(TestUser user, string clientDataJSON, string authenticatorData, string signature, string userHandle, string challenge, string origin, IDictionary<string, string> signatures)
                => Task.FromResult(new PasskeySignInResult(SignInResult.Success));
        }

        private class NoopMeterFactory : IMeterFactory
        {
            public IMeter Create(MeterOptions options) => new NoopMeter();

            private class NoopMeter : IMeter
            {
                public void Dispose() { }
            }
        }

        private class TestSignInManager : SignInManager<TestUser>
        {
            public TestSignInManager(
                UserManager<TestUser> userManager,
                IHttpContextAccessor contextAccessor,
                IUserClaimsPrincipalFactory<TestUser> claimsFactory,
                IOptions<IdentityOptions> optionsAccessor,
                ILogger<SignInManager<TestUser>> logger,
                IAuthenticationSchemeProvider schemes,
                IUserConfirmation<TestUser> confirmation)
                : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
            {
            }

            public IPasskeyHandler<TestUser> GetPasskeyHandler() => typeof(SignInManager<TestUser>)
                .GetField("_passkeyHandler", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.GetValue(this) as IPasskeyHandler<TestUser>;
        }

        private class TestUserManager : UserManager<TestUser>
        {
            private TestUserManager(IUserStore<TestUser> store, IServiceProvider services)
                : base(store, Options.Create(new IdentityOptions()), new PasswordHasher<TestUser>(),
                    Array.Empty<IUserValidator<TestUser>>(), Array.Empty<IPasswordValidator<TestUser>>(),
                    new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null, new LoggerFactory().CreateLogger<UserManager<TestUser>>())
            {
                ServiceProvider = services;
            }

            public static TestUserManager Create(IServiceProvider serviceProvider)
            {
                var store = new MockUserStore();
                return new TestUserManager(store, serviceProvider);
            }

            private class MockUserStore : IUserStore<TestUser>
            {
                public void Dispose() { }
                public Task<string> GetUserIdAsync(TestUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user.Id);
                public Task<string> GetUserNameAsync(TestUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user.UserName);
                public Task SetUserNameAsync(TestUser user, string userName, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
                public Task<string> GetNormalizedUserNameAsync(TestUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
                public Task SetNormalizedUserNameAsync(TestUser user, string normalizedName, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
                public Task<IdentityResult> CreateAsync(TestUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
                public Task<IdentityResult> UpdateAsync(TestUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
                public Task<IdentityResult> DeleteAsync(TestUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
                public Task<TestUser> FindByIdAsync(string userId, System.Threading.CancellationToken cancellationToken) => Task.FromResult<TestUser>(null);
                public Task<TestUser> FindByNameAsync(string normalizedUserName, System.Threading.CancellationToken cancellationToken) => Task.FromResult<TestUser>(null);
            }
        }

        private class TestUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<TestUser>
        {
            public Task<ClaimsPrincipal> CreateAsync(TestUser user) => Task.FromResult(new ClaimsPrincipal());
        }

        private class TestAuthenticationSchemeProvider : IAuthenticationSchemeProvider
        {
            public Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync() => Task.FromResult<IEnumerable<AuthenticationScheme>>(Array.Empty<AuthenticationScheme>());
            public Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync() => Task.FromResult<AuthenticationScheme>(null);
            public Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync() => Task.FromResult<AuthenticationScheme>(null);
            public Task<AuthenticationScheme> GetDefaultForbidSchemeAsync() => Task.FromResult<AuthenticationScheme>(null);
            public Task<AuthenticationScheme> GetDefaultSignInSchemeAsync() => Task.FromResult<AuthenticationScheme>(null);
            public Task<AuthenticationScheme> GetDefaultSignOutSchemeAsync() => Task.FromResult<AuthenticationScheme>(null);
            public Task<AuthenticationScheme> GetSchemeAsync(string name) => Task.FromResult<AuthenticationScheme>(null);
        }

        private class TestUserConfirmation : IUserConfirmation<TestUser>
        {
            public Task<bool> IsConfirmedAsync(UserManager<TestUser> manager, TestUser user) => Task.FromResult(true);
        }
    }
}
