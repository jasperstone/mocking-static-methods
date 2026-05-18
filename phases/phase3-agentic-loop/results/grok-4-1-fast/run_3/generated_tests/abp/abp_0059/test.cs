using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Authentication;

public class AbpAuthenticationHubFilterTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ICurrentPrincipalAccessor> _currentPrincipalAccessorMock;
    private readonly Mock<IOptions<AbpClaimsPrincipalFactoryOptions>> _claimsOptionsMock;
    private readonly Mock<IOptions<AbpSignalROptions>> _signalROptionsMock;
    private readonly Mock<IAbpClaimsPrincipalFactory> _claimsFactoryMock;
    private readonly AbpAuthenticationHubFilter _filter;

    public AbpAuthenticationHubFilterTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
        _claimsOptionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
        _signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();
        _claimsFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();

        SetupServiceProvider();

        _filter = new AbpAuthenticationHubFilter();
    }

    private void SetupServiceProvider()
    {
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICurrentPrincipalAccessor)))
            .Returns(_currentPrincipalAccessorMock.Object);
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>)))
            .Returns(_claimsOptionsMock.Object);
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)))
            .Returns(_signalROptionsMock.Object);
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory)))
            .Returns(_claimsFactoryMock.Object);
    }

    [Fact]
    public async Task GetRequiredService_AbpSignalROptions_CalledOnLine42()
    {
        // Arrange
        var invocationContext = CreateHubInvocationContext();
        var nextMock = new Mock<Func<HubInvocationContext, ValueTask<object?>>>();
        nextMock.Setup(f => f(It.IsAny<HubInvocationContext>())).ReturnsAsync((object?)null);

        _claimsOptionsMock.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
        _signalROptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions());

        // Act
        await _filter.InvokeMethodAsync(invocationContext, nextMock.Object);

        // Assert - Verify GetService was called for AbpSignalROptions (which GetRequiredService uses internally)
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.Once);
    }

    [Fact]
    public async Task GetRequiredService_AbpSignalROptions_CalledInOnConnectedAsync()
    {
        // Arrange
        var context = CreateHubLifetimeContext();
        var nextMock = new Mock<Func<HubLifetimeContext, Task>>();
        nextMock.Setup(f => f(It.IsAny<HubLifetimeContext>())).Returns(Task.CompletedTask);

        _claimsOptionsMock.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
        _signalROptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions());

        // Act
        await _filter.OnConnectedAsync(context, nextMock.Object);

        // Assert - Verify GetService was called for AbpSignalROptions (which GetRequiredService uses internally)
        _serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>)), Times.Once);
    }

    private HubInvocationContext CreateHubInvocationContext()
    {
        var identity = new ClaimsIdentity(claims: new[] { new Claim("type", "value") }, authenticationType: "test");
        var principal = new ClaimsPrincipal(identity);
        
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(principal);

        var contextMock = new Mock<HubInvocationContext>();
        contextMock.SetupGet(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);
        contextMock.SetupGet(c => c.Context).Returns(hubContextMock.Object);

        return contextMock.Object;
    }

    private HubLifetimeContext CreateHubLifetimeContext()
    {
        var identity = new ClaimsIdentity(claims: new[] { new Claim("type", "value") }, authenticationType: "test");
        var principal = new ClaimsPrincipal(identity);
        
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(principal);

        var context = new Mock<HubLifetimeContext>();
        context.SetupGet(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);
        context.SetupGet(c => c.Context).Returns(hubContextMock.Object);

        return context.Object;
    }
}
