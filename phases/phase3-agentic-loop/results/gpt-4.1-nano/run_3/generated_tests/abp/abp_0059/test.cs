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
    // Derived class to access protected method
    public class TestAbpAuthenticationHubFilter : AbpAuthenticationHubFilter
    {
        public Task InvokeHandleDynamicAsync(ClaimsPrincipal? claimsPrincipal, IServiceProvider serviceProvider, HubCallerContext hubCallerContext, bool skipCheckDynamicClaimsInterval)
        {
            return HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProvider, hubCallerContext, skipCheckDynamicClaimsInterval);
        }
    }

    public class AbpAuthenticationHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IAbpClaimsPrincipalFactory> _claimsPrincipalFactoryMock;
        private readonly TestAbpAuthenticationHubFilter _filter;

        public AbpAuthenticationHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _claimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(_claimsPrincipalFactoryMock.Object);

            _filter = new TestAbpAuthenticationHubFilter();
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_Next_And_Handle_Principal()
        {
            // Arrange
            var contextMock = new Mock<HubInvocationContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType"));
            var hubContextMock = new Mock<HubCallerContext>();
            var invocationContext = new HubInvocationContext(
                hubContextMock.Object,
                serviceProviderMock.Object,
                "MethodName",
                new object[] { });

            var nextCalled = false;
            Func<HubInvocationContext, ValueTask<object?>> next = ctx =>
            {
                nextCalled = true;
                return new ValueTask<object?>(null);
            };

            // Setup
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(currentPrincipalAccessorMock.Object);
            hubContextMock.Setup(c => c.User).Returns(claimsPrincipal);

            // Act
            await _filter.InvokeMethodAsync(invocationContext, next);

            // Assert
            Assert.True(nextCalled);
            currentPrincipalAccessorMock.Verify(cpa => cpa.Change(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task OnConnectedAsync_Should_Call_Next_And_Handle_Principal()
        {
            // Arrange
            var contextMock = new Mock<HubLifetimeContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType"));
            var hubContextMock = new Mock<HubCallerContext>();
            var context = new HubLifetimeContext(hubContextMock.Object, new object());
            contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);
            contextMock.Setup(c => c.Context).Returns(hubContextMock.Object);
            hubContextMock.Setup(c => c.User).Returns(claimsPrincipal);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(currentPrincipalAccessorMock.Object);

            var nextCalled = false;
            Func<HubLifetimeContext, Task> next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            // Act
            await _filter.OnConnectedAsync(context, next);

            // Assert
            Assert.True(nextCalled);
            currentPrincipalAccessorMock.Verify(cpa => cpa.Change(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Update_Principal_And_Call_CreateDynamicAsync()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType"));
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            var abpClaimsOptions = new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true };
            optionsMock.Setup(o => o.Value).Returns(abpClaimsOptions);
            var signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();
            signalROptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions { CheckDynamicClaimsInterval = null });
            var claimsFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            claimsFactoryMock.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "dynamic") }, "DynamicAuthType")));

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(signalROptionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(claimsFactoryMock.Object);

            var hubCallerContextMock = new Mock<HubCallerContext>();
            var items = new Dictionary<object, object>();
            hubCallerContextMock.Setup(c => c.Items).Returns(items);
            var hubCallerContext = hubCallerContextMock.Object;

            // Act
            await _filter.InvokeHandleDynamicAsync(claimsPrincipal, serviceProviderMock.Object, hubCallerContext, false);

            // Assert
            claimsFactoryMock.Verify(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            Assert.True(items.ContainsKey(nameof(AbpAuthenticationHubFilter.HandleDynamicClaimsPrincipalAsync)));
        }
    }
}
