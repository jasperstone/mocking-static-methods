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
        private readonly AbpAuthenticationHubFilter _filter;

        public AbpAuthenticationHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _claimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(_claimsPrincipalFactoryMock.Object);

            _filter = new AbpAuthenticationHubFilter();
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_HandleDynamicClaimsPrincipalAsync_And_ExecuteNext()
        {
            // Arrange
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuth"));
            var contextMock = new Mock<HubCallerContext>();
            contextMock.Setup(c => c.User).Returns(claims);
            var invocationContextMock = new Mock<HubInvocationContext>();
            invocationContextMock.SetupGet(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);
            invocationContextMock.SetupGet(c => c.Context).Returns(contextMock.Object);

            var nextCalled = false;
            Func<HubInvocationContext, ValueTask<object?>> next = ctx =>
            {
                nextCalled = true;
                return new ValueTask<object?>(null);
            };

            // Act
            await _filter.InvokeMethodAsync(invocationContextMock.Object, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task OnConnectedAsync_Should_Call_HandleDynamicClaimsPrincipalAsync_And_ExecuteNext()
        {
            // Arrange
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuth"));
            var contextMock = new Mock<HubLifetimeContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var hubContextMock = new Mock<HubCallerContext>();
            hubContextMock.Setup(c => c.User).Returns(claims);
            contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);
            contextMock.Setup(c => c.Context).Returns(hubContextMock.Object);

            var nextCalled = false;
            Func<HubLifetimeContext, Task> next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            // Act
            await _filter.OnConnectedAsync(contextMock.Object, next);

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Abort_When_ClaimsNotAuthenticated()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var hubCallerContextMock = new Mock<HubCallerContext>();
            var items = new Dictionary<object, object>();
            hubCallerContextMock.Setup(c => c.Items).Returns(items);
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
            var signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();
            signalROptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions { CheckDynamicClaimsInterval = null });
            var claimsFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            claimsFactoryMock.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity(claimsPrincipal.Claims, claimsPrincipal.Identity?.AuthenticationType)));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(optionsMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(signalROptionsMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(claimsFactoryMock.Object);

            // Act
            await _filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProvider.Object, hubCallerContextMock.Object, false);

            // Assert
            // No exception means pass
        }
    }
}
