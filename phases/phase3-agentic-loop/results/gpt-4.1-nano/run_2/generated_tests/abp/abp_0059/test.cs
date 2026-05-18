using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.Security.Claims;
using Volo.Abp.AspNetCore.SignalR.Authentication;

namespace Volo.Abp.AspNetCore.SignalR.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IAbpClaimsPrincipalFactory> _claimsPrincipalFactoryMock;
        private readonly Mock<IOptions<AbpClaimsPrincipalFactoryOptions>> _claimsPrincipalFactoryOptionsMock;
        private readonly Mock<IOptions<AbpSignalROptions>> _signalROptionsMock;
        private readonly AbpAuthenticationHubFilter _filter;

        public AbpAuthenticationHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _claimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            _claimsPrincipalFactoryOptionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            _signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();

            _claimsPrincipalFactoryOptionsMock.Setup(x => x.Value).Returns(new AbpClaimsPrincipalFactoryOptions { });
            _signalROptionsMock.Setup(x => x.Value).Returns(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(5) });

            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(_claimsPrincipalFactoryOptionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(_signalROptionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(_claimsPrincipalFactoryMock.Object);

            _filter = new AbpAuthenticationHubFilter();
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_HandleDynamicClaimsPrincipalAsync_And_Execute_Next()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType"));
            var hubContextMock = new Mock<HubCallerContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var hubInvocationContext = new HubInvocationContext(
                hubContextMock.Object,
                serviceProviderMock.Object,
                "MethodName",
                new object[] { });

            var nextCalled = false;
            Func<HubInvocationContext, ValueTask<object?>> next = ctx =>
            {
                nextCalled = true;
                return new ValueTask<object?>(42);
            };

            // Setup
            var currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
            currentPrincipalAccessorMock.Setup(c => c.Change(It.IsAny<ClaimsPrincipal>())).Returns(new DummyDisposable());

            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(currentPrincipalAccessorMock.Object);

            hubContextMock.Setup(c => c.User).Returns(claimsPrincipal);

            // Act
            var result = await _filter.InvokeMethodAsync(hubInvocationContext, next);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task OnConnectedAsync_Should_Call_HandleDynamicClaimsPrincipalAsync_And_Execute_Next()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType"));
            var contextMock = new Mock<HubLifetimeContext>();
            var hubContextMock = new Mock<HubCallerContext>();
            var context = new HubLifetimeContext(hubContextMock.Object, new object());

            var nextCalled = false;
            Func<HubLifetimeContext, Task> next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            // Setup
            var currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
            currentPrincipalAccessorMock.Setup(c => c.Change(It.IsAny<ClaimsPrincipal>())).Returns(new DummyDisposable());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(currentPrincipalAccessorMock.Object);

            hubContextMock.Setup(c => c.User).Returns(claimsPrincipal);
            contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);
            contextMock.Setup(c => c.Context).Returns(hubContextMock.Object);

            // Act
            await _filter.OnConnectedAsync(contextMock.Object, next);

            // Assert
            Assert.True(nextCalled);
        }

        private class DummyDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
