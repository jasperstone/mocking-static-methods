using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Authentication.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        private class TestCurrentPrincipalAccessor : ICurrentPrincipalAccessor
        {
            private ClaimsPrincipal _principal;

            public TestCurrentPrincipalAccessor(ClaimsPrincipal principal)
            {
                _principal = principal;
            }

            public IDisposable Change(ClaimsPrincipal principal)
            {
                var original = _principal;
                _principal = principal;
                return new DisposeAction(() => _principal = original);
            }

            private class DisposeAction : IDisposable
            {
                private readonly Action _action;
                public DisposeAction(Action action) => _action = action;
                public void Dispose() => _action();
            }
        }

        private class TestHubCallerContext : HubCallerContext
        {
            public override string ConnectionId { get; }
            public override string UserIdentifier { get; }
            public override ClaimsPrincipal User { get; }
            public override IDictionary<object, object> Items { get; }
            public override CancellationToken ConnectionAborted { get; }
            public override IServiceProvider RequestServices { get; set; }
            public bool Aborted { get; private set; }

            public TestHubCallerContext(ClaimsPrincipal user)
            {
                User = user;
                Items = new Dictionary<object, object>();
            }

            public override void Abort()
            {
                Aborted = true;
            }
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_GetRequiredService_And_Invoke_Next()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType") { });
            var currentPrincipalAccessor = new TestCurrentPrincipalAccessor(claimsPrincipal);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ICurrentPrincipalAccessor)))
                .Returns(currentPrincipalAccessor);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(currentPrincipalAccessor);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false }));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(Options.Create(new AbpSignalROptions()));

            var hubCallerContext = new TestHubCallerContext(claimsPrincipal)
            {
                RequestServices = serviceProviderMock.Object
            };

            var invocationContext = new HubInvocationContext(
                hubCallerContext,
                serviceProviderMock.Object,
                "TestMethod",
                Array.Empty<object>());

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            async ValueTask<object?> Next(HubInvocationContext ctx)
            {
                nextCalled = true;
                return "result";
            }

            // Act
            var result = await filter.InvokeMethodAsync(invocationContext, Next);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal("result", result);
        }

        [Fact]
        public async Task OnConnectedAsync_Should_Call_GetRequiredService_And_Invoke_Next()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType") { });
            var currentPrincipalAccessor = new TestCurrentPrincipalAccessor(claimsPrincipal);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(currentPrincipalAccessor);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false }));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(Options.Create(new AbpSignalROptions()));

            var hubCallerContext = new TestHubCallerContext(claimsPrincipal)
            {
                RequestServices = serviceProviderMock.Object
            };

            var lifetimeContext = new HubLifetimeContext(hubCallerContext, serviceProviderMock.Object);

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            Task Next(HubLifetimeContext ctx)
            {
                nextCalled = true;
                return Task.CompletedTask;
            }

            // Act
            await filter.OnConnectedAsync(lifetimeContext, Next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Abort_When_ClaimsPrincipal_Is_Not_Authenticated_After_CreateDynamicAsync()
        {
            // Arrange
            var identity = new ClaimsIdentity("TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var abpClaimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            abpClaimsPrincipalFactoryMock.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity())); // Not authenticated identity

            var abpClaimsPrincipalFactoryOptions = Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
            var abpSignalROptions = Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromSeconds(1) });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(abpClaimsPrincipalFactoryOptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(abpSignalROptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(abpClaimsPrincipalFactoryMock.Object);

            var hubCallerContext = new TestHubCallerContext(claimsPrincipal);

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProviderMock.Object, hubCallerContext, false);

            // Assert
            Assert.True(hubCallerContext.Aborted);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Not_Abort_When_ClaimsPrincipal_Is_Authenticated_After_CreateDynamicAsync()
        {
            // Arrange
            var identity = new ClaimsIdentity("TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var abpClaimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            abpClaimsPrincipalFactoryMock.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity("TestAuthType"))); // Authenticated identity

            var abpClaimsPrincipalFactoryOptions = Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
            var abpSignalROptions = Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromSeconds(1) });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(abpClaimsPrincipalFactoryOptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(abpSignalROptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(abpClaimsPrincipalFactoryMock.Object);

            var hubCallerContext = new TestHubCallerContext(claimsPrincipal);

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProviderMock.Object, hubCallerContext, false);

            // Assert
            Assert.False(hubCallerContext.Aborted);
        }
    }
}
