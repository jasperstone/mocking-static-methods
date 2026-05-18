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
                _principal = principal;
                return new DisposeAction(() => { });
            }

            private class DisposeAction : IDisposable
            {
                private readonly Action _dispose;
                public DisposeAction(Action dispose) => _dispose = dispose;
                public void Dispose() => _dispose();
            }
        }

        [Fact]
        public async Task InvokeMethodAsync_CallsGetRequiredServiceAndNext()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType") { });
            var currentPrincipalAccessor = new TestCurrentPrincipalAccessor(claimsPrincipal);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ICurrentPrincipalAccessor)))
                .Returns(currentPrincipalAccessor);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
                .Returns(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false }));

            var hubCallerContextMock = new Mock<HubCallerContext>();
            hubCallerContextMock.SetupGet(c => c.User).Returns(claimsPrincipal);
            hubCallerContextMock.SetupGet(c => c.Items).Returns(new Dictionary<string, object>());

            var invocationContext = new HubInvocationContext(
                hubCallerContextMock.Object,
                serviceProviderMock.Object,
                "TestMethod",
                Array.Empty<object>());

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            ValueTask<object?> Next(HubInvocationContext ctx)
            {
                nextCalled = true;
                return new ValueTask<object?>(Task.FromResult<object?>("result"));
            }

            // Act
            var result = await filter.InvokeMethodAsync(invocationContext, Next);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal("result", result);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(ICurrentPrincipalAccessor)), Times.Once);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_DoesNotAbort_WhenNotAuthenticated()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
                .Returns(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true }));
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptions<AbpSignalROptions>)))
                .Returns(Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(1) }));
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IAbpClaimsPrincipalFactory)))
                .Returns(Mock.Of<IAbpClaimsPrincipalFactory>());

            var hubCallerContextMock = new Mock<HubCallerContext>();
            hubCallerContextMock.SetupGet(c => c.Items).Returns(new Dictionary<string, object>());

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProviderMock.Object, hubCallerContextMock.Object, false);

            // Assert
            // No exception and no abort called
            hubCallerContextMock.Verify(c => c.Abort(), Times.Never);
        }
    }
}
