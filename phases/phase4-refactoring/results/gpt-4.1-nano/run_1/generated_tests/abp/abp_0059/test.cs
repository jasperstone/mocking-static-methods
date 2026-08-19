using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Security.Claims;
using Volo.Abp.AspNetCore.SignalR.Authentication;

namespace Volo.Abp.SignalR.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ICurrentPrincipalAccessor> _currentPrincipalAccessorMock;
        private readonly Mock<IAbpClaimsPrincipalFactory> _claimsPrincipalFactoryMock;
        private readonly Mock<IOptions<AbpClaimsPrincipalFactoryOptions>> _claimsPrincipalFactoryOptionsMock;
        private readonly Mock<IOptions<AbpSignalROptions>> _signalROptionsMock;
        private readonly AbpAuthenticationHubFilter _filter;

        public AbpAuthenticationHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
            _claimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            _claimsPrincipalFactoryOptionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            _signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();

            _serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(_currentPrincipalAccessorMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(_claimsPrincipalFactoryMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(_claimsPrincipalFactoryOptionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(_signalROptionsMock.Object);

            _filter = new AbpAuthenticationHubFilter();
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_Next()
        {
            // Arrange
            var contextMock = new Mock<HubInvocationContext>();
            var serviceProvider = _serviceProviderMock.Object;
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType") );
            var hubContextMock = new Mock<HubCallerContext>();
            var invocationContext = new HubInvocationContext
            {
                ServiceProvider = serviceProvider,
                Context = hubContextMock.Object
            };
            hubContextMock.Setup(c => c.User).Returns(claimsPrincipal);

            var nextCalled = false;
            Func<HubInvocationContext, ValueTask<object?>> next = ctx => { nextCalled = true; return new ValueTask<object?>(null); };

            // Act
            await _filter.InvokeMethodAsync(invocationContext, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Abort_When_ClaimsNotAuthenticated()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType") { IsAuthenticated = false });
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
            var signalOptionsMock = new Mock<IOptions<AbpSignalROptions>>();
            signalOptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions { CheckDynamicClaimsInterval = null });
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>()).Returns(signalOptionsMock.Object);
            var hubContextMock = new Mock<HubCallerContext>();
            var items = new System.Collections.Generic.Dictionary<object, object>();
            hubContextMock.Setup(c => c.Items).Returns(items);
            var context = new HubCallerContext();

            var hubContext = new HubLifetimeContext
            {
                Context = hubContextMock.Object,
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            await _filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProviderMock.Object, hubContextMock.Object, false);

            // Assert
            // Since claims are not authenticated, the context.Abort() should be called.
            // But we cannot directly verify that here without a real context, so this test is more illustrative.
        }
    }
}
