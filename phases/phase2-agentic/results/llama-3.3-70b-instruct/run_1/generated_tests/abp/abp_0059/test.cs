using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        [Fact]
        public async Task InvokeMethodAsync_ValidClaimsPrincipal_DoesNotAbortHub()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
                .BuildServiceProvider();

            var invocationContext = new HubInvocationContext
            {
                ServiceProvider = serviceProvider,
                Context = new HubCallerContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            var next = Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>();

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.InvokeMethodAsync(invocationContext, next);

            // Assert
            Assert.False(invocationContext.Context.Aborted);
        }

        [Fact]
        public async Task OnConnectedAsync_ValidClaimsPrincipal_DoesNotAbortHub()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
                .BuildServiceProvider();

            var context = new HubLifetimeContext
            {
                ServiceProvider = serviceProvider,
                Context = new HubCallerContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            var next = Mock.Of<Func<HubLifetimeContext, Task>>();

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.OnConnectedAsync(context, next);

            // Assert
            Assert.False(context.Context.Aborted);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_UnauthenticatedClaimsPrincipal_DoesNotAbortHub()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
                .BuildServiceProvider();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            var hubCallerContext = new HubCallerContext
            {
                User = claimsPrincipal
            };

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProvider, hubCallerContext, false);

            // Assert
            Assert.False(hubCallerContext.Aborted);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_AuthenticatedClaimsPrincipalButNotDynamicClaimsEnabled_DoesNotAbortHub()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
                .BuildServiceProvider();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            var hubCallerContext = new HubCallerContext
            {
                User = claimsPrincipal
            };

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProvider, hubCallerContext, false);

            // Assert
            Assert.False(hubCallerContext.Aborted);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_AuthenticatedClaimsPrincipalAndDynamicClaimsEnabledButCheckIntervalNotPassed_DoesNotAbortHub()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
                .BuildServiceProvider();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            var hubCallerContext = new HubCallerContext
            {
                User = claimsPrincipal,
                Items = new Dictionary<object, object>
                {
                    { nameof(HandleDynamicClaimsPrincipalAsync), DateTime.UtcNow }
                }
            };

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProvider, hubCallerContext, false);

            // Assert
            Assert.False(hubCallerContext.Aborted);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_AuthenticatedClaimsPrincipalAndDynamicClaimsEnabledAndCheckIntervalPassedButCreateDynamicFails_DoesAbortHub()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
                .BuildServiceProvider();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            var hubCallerContext = new HubCallerContext
            {
                User = claimsPrincipal
            };

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProvider, hubCallerContext, false);

            // Assert
            Assert.True(hubCallerContext.Aborted);
        }
    }
}
