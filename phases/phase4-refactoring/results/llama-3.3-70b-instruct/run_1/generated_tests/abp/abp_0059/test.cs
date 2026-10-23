using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        [Fact]
        public async Task InvokeMethodAsync_ValidClaimsPrincipal_DoesNotAbort()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var currentPrincipalAccessor = Mock.Of<ICurrentPrincipalAccessor>();
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
            var hub = Mock.Of<Microsoft.AspNetCore.SignalR.Hub>();
            var methodInfo = typeof(string).GetMethod("ToString");
            var parameters = new object[] { };
            var invocationContext = new HubInvocationContext(new HubCallerContext(serviceProvider, claimsPrincipal), serviceProvider, hub, methodInfo, parameters);
            var next = Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>();
            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.InvokeMethodAsync(invocationContext, next);

            // Assert
            Assert.False(invocationContext.Context.Aborted);
        }

        [Fact]
        public async Task OnConnectedAsync_ValidClaimsPrincipal_DoesNotAbort()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var currentPrincipalAccessor = Mock.Of<ICurrentPrincipalAccessor>();
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
            var hub = Mock.Of<Microsoft.AspNetCore.SignalR.Hub>();
            var context = new HubLifetimeContext(new HubCallerContext(serviceProvider, claimsPrincipal), serviceProvider, hub);
            var next = Mock.Of<Func<HubLifetimeContext, Task>>();
            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.OnConnectedAsync(context, next);

            // Assert
            Assert.False(context.Context.Aborted);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_UnauthenticatedClaimsPrincipal_DoesNotCreateDynamicClaims()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
            var hubCallerContext = new HubCallerContext(serviceProvider, claimsPrincipal);
            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProvider, hubCallerContext, false);

            // Assert
            Assert.Null(hubCallerContext.Items[nameof(AbpAuthenticationHubFilter.HandleDynamicClaimsPrincipalAsync)]);
        }
    }
}
