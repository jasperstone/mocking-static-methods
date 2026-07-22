using System;
using System.Collections.Generic;
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
            .Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
            .Returns(_currentPrincipalAccessorMock.Object);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .Returns(_claimsOptionsMock.Object);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
            .Returns(_signalROptionsMock.Object);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
            .Returns(_claimsFactoryMock.Object);
    }

    [Fact]
    public async Task InvokeMethodAsync_ShouldCallGetRequiredService_ForAbpSignalROptions()
    {
        // Arrange
        var invocationContext = CreateInvocationContext();
        bool nextCalled = false;
        _claimsOptionsMock.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
        _signalROptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions());

        // Act
        await _filter.InvokeMethodAsync(invocationContext, async ctx => { nextCalled = true; return null!; });

        // Assert
        _serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Once);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldCallGetRequiredService_ForAbpSignalROptions()
    {
        // Arrange
        var context = CreateHubLifetimeContext();
        bool nextCalled = false;
        _claimsOptionsMock.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
        _signalROptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions());

        // Act
        await _filter.OnConnectedAsync(context, async ctx => { nextCalled = true; return; });

        // Assert
        _serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Once);
        Assert.True(nextCalled);
    }

    private HubInvocationContext CreateInvocationContext()
    {
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
        hubContextMock.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("test", "test") }, "test")));

        var hubMock = new Mock<Hub>().Object;

        return new HubInvocationContext(
            hubContextMock.Object,
            _serviceProviderMock.Object,
            hubMock,
            "Test",
            Array.Empty<object>()
        );
    }

    private HubLifetimeContext CreateHubLifetimeContext()
    {
        var contextMock = new Mock<HubCallerContext>();
        contextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
        contextMock.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("test", "test") }, "test")));

        var hubMock = new Mock<Hub>().Object;

        return new HubLifetimeContext(
            contextMock.Object,
            _serviceProviderMock.Object,
            hubMock
        );
    }
}
