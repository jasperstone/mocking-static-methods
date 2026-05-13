using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests;

public class AbpAuthenticationHubFilterTests
{
    [Fact]
    public async Task InvokeMethodAsync_WithAuthenticatedUser_CallsHandleDynamicClaimsPrincipalAsync()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var invocationContext = new HubInvocationContext(
            Mock.Of<HubCallerContext>(),
            serviceProvider,
            Mock.Of<Func<HubInvocationContext, ValueTask<object?>>>());

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.InvokeMethodAsync(invocationContext, async context => null);

        // Assert
        Mock.Get(invocationContext.ServiceProvider.GetService<IAbpClaimsPrincipalFactory>())
            .Verify(factory => factory.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_WithAuthenticatedUser_CallsHandleDynamicClaimsPrincipalAsync()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Mock.Of<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .AddSingleton<IOptions<AbpSignalROptions>>(Mock.Of<IOptions<AbpSignalROptions>>())
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var context = new HubLifetimeContext(
            Mock.Of<HubCallerContext>(),
            serviceProvider,
            Mock.Of<Func<HubLifetimeContext, Task>>());

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.OnConnectedAsync(context, async ctx => { });

        // Assert
        Mock.Get(context.ServiceProvider.GetService<IAbpClaimsPrincipalFactory>())
            .Verify(factory => factory.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }

    [Fact]
    public async Task HandleDynamicClaimsPrincipalAsync_WithAuthenticatedUserAndEnabledDynamicClaims_CallsCreateDynamicAsync()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ICurrentPrincipalAccessor>(Mock.Of<ICurrentPrincipalAccessor>())
            .AddSingleton<IOptions<AbpClaimsPrincipalFactoryOptions>>(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true }))
            .AddSingleton<IOptions<AbpSignalROptions>>(Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(1) }))
            .AddSingleton<IAbpClaimsPrincipalFactory>(Mock.Of<IAbpClaimsPrincipalFactory>())
            .BuildServiceProvider();

        var hubCallerContext = new HubCallerContext(
            new DefaultHubCallerContext(
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") })),
                Mock.Of<IHubProtocol>(),
                Mock.Of<IHubConnectionContextAccessor>()));

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.HandleDynamicClaimsPrincipalAsync(hubCallerContext.User, serviceProvider, hubCallerContext, false);

        // Assert
        Mock.Get(serviceProvider.GetService<IAbpClaimsPrincipalFactory>())
            .Verify(factory => factory.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }
}
