using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IdentityApiTests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        private class DummyUser { }

        [Fact]
        public void MapIdentityApi_RegistersServicesAndEndpoints()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add required services
            services.AddSingleton(new TimeProvider());
            services.AddOptions<BearerTokenOptions>().Configure(opt => { });
            services.AddTransient<IEmailSender<DummyUser>, DummyEmailSender>();
            services.AddTransient<LinkGenerator, DummyLinkGenerator>();
            services.AddTransient<UserManager<DummyUser>, DummyUserManager>();
            services.AddTransient<IUserStore<DummyUser>, DummyUserStore>();
            services.AddTransient<SignInManager<DummyUser>, DummySignInManager>();
            services.AddTransient<IServiceProvider>(sp => sp);

            var serviceProvider = services.BuildServiceProvider();

            var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
            mockEndpointRouteBuilder.SetupGet(b => b.ServiceProvider).Returns(serviceProvider);
            var routeBuilder = mockEndpointRouteBuilder.Object;

            // Act
            var builder = routeBuilder.MapIdentityApi<DummyUser>();

            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void MapIdentityApi_CallsGetRequiredService_ForTimeProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddSingleton(new TimeProvider());
            services.AddOptions<BearerTokenOptions>().Configure(opt => { });
            services.AddTransient<IEmailSender<DummyUser>, DummyEmailSender>();
            services.AddTransient<LinkGenerator, DummyLinkGenerator>();
            services.AddTransient<UserManager<DummyUser>, DummyUserManager>();
            services.AddTransient<IUserStore<DummyUser>, DummyUserStore>();
            services.AddTransient<SignInManager<DummyUser>, DummySignInManager>();
            services.AddTransient<IServiceProvider>(sp => sp);

            var serviceProvider = services.BuildServiceProvider();

            var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
            mockEndpointRouteBuilder.SetupGet(b => b.ServiceProvider).Returns(serviceProvider);
            var routeBuilder = mockEndpointRouteBuilder.Object;

            // Act
            var builder = routeBuilder.MapIdentityApi<DummyUser>();

            // Assert
            Assert.NotNull(builder);
        }

        // Dummy classes for dependencies
        private class DummyEmailSender : IEmailSender<DummyUser> { }
        private class DummyLinkGenerator : LinkGenerator { }
        private class DummyUserManager : UserManager<DummyUser>
        {
            public DummyUserManager() : base(Mock.Of<IUserStore<DummyUser>>(), null, null, null, null, null, null, null, null) { }
        }
        private class DummyUserStore : IUserStore<DummyUser>
        {
            public Task<IdentityResult> CreateAsync(DummyUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
            public Task<IdentityResult> DeleteAsync(DummyUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
            public void Dispose() { }
            public Task<DummyUser> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult(new DummyUser());
            public Task<DummyUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult(new DummyUser());
            public Task<string> GetNormalizedUserNameAsync(DummyUser user, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
            public Task<string> GetUserIdAsync(DummyUser user, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
            public Task<string> GetUserNameAsync(DummyUser user, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
            public Task SetNormalizedUserNameAsync(DummyUser user, string normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
            public Task SetUserNameAsync(DummyUser user, string userName, CancellationToken cancellationToken) => Task.CompletedTask;
            public Task<IdentityResult> UpdateAsync(DummyUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        }
        private class DummySignInManager : SignInManager<DummyUser>
        {
            public DummySignInManager() : base(Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(), null, null, null, null, null, null) { }
            public override Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure) => Task.FromResult(SignInResult.Success);
            public override Task<bool> ValidateSecurityStampAsync(DummyUser user) => Task.FromResult(true);
            public override Task<ClaimsPrincipal> CreateUserPrincipalAsync(DummyUser user) => Task.FromResult(new ClaimsPrincipal());
        }
    }
}
