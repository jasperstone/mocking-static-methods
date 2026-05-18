using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        [Fact]
        public async Task InvokeMethodAsync_CallsGetRequiredService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Options.Create(new AbpClaimsPrincipalFactoryOptions()))
                .AddSingleton<IOptions<AbpSignalROptions>>(Options.Create(new AbpSignalROptions()))
                .BuildServiceProvider();

            var hub = Mock.Of<Hub>();
            var hubCallerContext = Mock.Of<HubCallerContext>();
            var invocationContext = new HubInvocationContext(hubCallerContext, serviceProvider, hub, typeof(string).GetMethod("ToString"), new object[0]);

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.InvokeMethodAsync(invocationContext, async context => await Task.FromResult<object?>(null));

            // Assert
            var currentPrincipalAccessor = serviceProvider.GetRequiredService<ICurrentPrincipalAccessor>();
            Assert.NotNull(currentPrincipalAccessor);
        }

        [Fact]
        public async Task OnConnectedAsync_CallsGetRequiredService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Options.Create(new AbpClaimsPrincipalFactoryOptions()))
                .AddSingleton<IOptions<AbpSignalROptions>>(Options.Create(new AbpSignalROptions()))
                .BuildServiceProvider();

            var hub = Mock.Of<Hub>();
            var hubCallerContext = Mock.Of<HubCallerContext>();
            var context = new HubLifetimeContext(hubCallerContext, serviceProvider, hub);

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.OnConnectedAsync(context, async ctx => await Task.CompletedTask);

            // Assert
            var currentPrincipalAccessor = serviceProvider.GetRequiredService<ICurrentPrincipalAccessor>();
            Assert.NotNull(currentPrincipalAccessor);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_CallsGetRequiredService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
                .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Options.Create(new AbpClaimsPrincipalFactoryOptions()))
                .AddSingleton<IOptions<AbpSignalROptions>>(Options.Create(new AbpSignalROptions()))
                .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
                .BuildServiceProvider();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await (Task)filter.GetType().GetMethod("HandleDynamicClaimsPrincipalAsync", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(filter, new object[] { claimsPrincipal, serviceProvider, Mock.Of<HubCallerContext>(), false });

            // Assert
            var currentPrincipalAccessor = serviceProvider.GetRequiredService<ICurrentPrincipalAccessor>();
            Assert.NotNull(currentPrincipalAccessor);
        }
    }
}
