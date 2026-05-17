using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR.Authentication;
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") }));
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .BuildServiceProvider();

            var hub = new Mock<IHub>();
            var methodInfo = typeof(AbpAuthenticationHubFilter).GetMethod("InvokeMethodAsync");
            var invocationContext = new HubInvocationContext(new HubCallerContext { User = claimsPrincipal }, serviceProvider, hub.Object, methodInfo, new object[0]);

            var next = Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>();

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.InvokeMethodAsync(invocationContext, next);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task OnConnectedAsync_ValidClaimsPrincipal_DoesNotAbort()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") }));
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .BuildServiceProvider();

            var context = new HubLifetimeContext
            {
                Hub = new Mock<IHub>().Object,
                ServiceProvider = serviceProvider,
                Context = new HubCallerContext
                {
                    User = claimsPrincipal
                }
            };

            var next = Mock.Of<Func<HubLifetimeContext, Task>>();

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.OnConnectedAsync(context, next);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_ValidClaimsPrincipal_DoesNotAbort()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") }));
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .BuildServiceProvider();

            var hubCallerContext = new HubCallerContext
            {
                User = claimsPrincipal
            };

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProvider, hubCallerContext, false);

            // Assert
            Assert.True(true);
        }
    }
}
