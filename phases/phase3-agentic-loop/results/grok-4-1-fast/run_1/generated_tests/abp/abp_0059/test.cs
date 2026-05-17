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
    private readonly Mock<IDisposable> _changeDisposableMock;
    private readonly Mock<IOptions<AbpClaimsPrincipalFactoryOptions>> _claimsOptionsMock;
    private readonly Mock<IOptions<AbpSignalROptions>> _signalROptionsMock;
    private readonly Mock<IAbpClaimsPrincipalFactory> _claimsFactoryMock;
    private readonly AbpAuthenticationHubFilter _filter;

    public AbpAuthenticationHubFilterTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
        _changeDisposableMock = new Mock<IDisposable>();
        _claimsOptionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
        _signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();
        _claimsFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();

        SetupServiceProvider();

        _filter = new AbpAuthenticationHubFilter();
    }

    private void SetupServiceProvider()
    {
        _serviceProviderMock
            .Setup(x => x.GetRequiredService<ICurrentPrincipalAccessor>())
            .Returns(_currentPrincipalAccessorMock.Object);
        _serviceProviderMock
            .Setup(x => x.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .Returns(_claimsOptionsMock.Object);
        _serviceProviderMock
            .Setup(x => x.GetRequiredService<IOptions<AbpSignalROptions>>())
            .Returns(_signalROptionsMock.Object);
        _serviceProviderMock
            .Setup(x => x.GetRequiredService<IAbpClaimsPrincipalFactory>())
            .Returns(_claimsFactoryMock.Object);
    }

    [Fact]
    public async void HandleDynamicClaimsPrincipalAsync_ShouldCallGetRequiredService_ForAbpSignalROptions_WhenConditionsMet()
    {
        // Arrange - using reflection to call protected method
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, "test"));
        var hubContext = new Mock<HubCallerContext>();
        hubContext.Setup(x => x.User).Returns(claimsPrincipal);
        _claimsOptionsMock.Setup(x => x.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
        _signalROptionsMock.Setup(x => x.Value).Returns(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromSeconds(5) });
        _claimsFactoryMock.Setup(x => x.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(claimsPrincipal);

        // Act
        var method = typeof(AbpAuthenticationHubFilter).GetMethod("HandleDynamicClaimsPrincipalAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        await (Task)method.Invoke(_filter, new object?[] { claimsPrincipal, _serviceProviderMock.Object, hubContext.Object, true })!;

        // Assert
        _serviceProviderMock.Verify(x => x.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Once);
    }

    [Fact]
    public async void HandleDynamicClaimsPrincipalAsync_ShouldNotCallGetRequiredService_ForAbpSignalROptions_WhenDynamicClaimsDisabled()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, "test"));
        var hubContext = new Mock<HubCallerContext>();
        _claimsOptionsMock.Setup(x => x.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false });

        // Act
        var method = typeof(AbpAuthenticationHubFilter).GetMethod("HandleDynamicClaimsPrincipalAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        await (Task)method.Invoke(_filter, new object?[] { claimsPrincipal, _serviceProviderMock.Object, hubContext.Object, true })!;

        // Assert
        _serviceProviderMock.Verify(x => x.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Never);
    }

    [Fact]
    public async void HandleDynamicClaimsPrincipalAsync_ShouldNotCallGetRequiredService_ForAbpSignalROptions_WhenNotAuthenticated()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var hubContext = new Mock<HubCallerContext>();

        // Act
        var method = typeof(AbpAuthenticationHubFilter).GetMethod("HandleDynamicClaimsPrincipalAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        await (Task)method.Invoke(_filter, new object?[] { claimsPrincipal, _serviceProviderMock.Object, hubContext.Object, true })!;

        // Assert
        _serviceProviderMock.Verify(x => x.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Never);
    }

    [Fact]
    public async void InvokeMethodAsync_ShouldCallGetRequiredService_ForAbpSignalROptions_WhenConditionsMet()
    {
        // Arrange
        var invocationContext = CreateInvocationContext(true);
        var nextCalled = false;
        _currentPrincipalAccessorMock.Setup(x => x.Change(It.IsAny<ClaimsPrincipal>()))
            .Returns(_changeDisposableMock.Object);

        // Act
        await _filter.InvokeMethodAsync(invocationContext, async ctx => { nextCalled = true; return null!; });

        // Assert
        _serviceProviderMock.Verify(x => x.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Once);
        Assert.True(nextCalled);
    }

    [Fact]
    public async void OnConnectedAsync_ShouldCallGetRequiredService_ForAbpSignalROptions_WhenConditionsMet()
    {
        // Arrange
        var context = CreateHubLifetimeContext(true);
        var nextCalled = false;
        _currentPrincipalAccessorMock.Setup(x => x.Change(It.IsAny<ClaimsPrincipal>()))
            .Returns(_changeDisposableMock.Object);

        // Act
        await _filter.OnConnectedAsync(context, async ctx => { nextCalled = true; });

        // Assert
        _serviceProviderMock.Verify(x => x.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Once);
        Assert.True(nextCalled);
    }

    private HubInvocationContext CreateInvocationContext(bool authenticated)
    {
        var hubContext = new Mock<HubCallerContext>();
        if (authenticated)
        {
            hubContext.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, "test")));
        }
        var hubClients = new Mock<IHubCallerClients>();
        return new HubInvocationContext(
            _serviceProviderMock.Object,
            hubContext.Object,
            Array.Empty<object>(),
            "TestHub",
            "TestMethod",
            Array.Empty<object>()
        );
    }

    private HubLifetimeContext CreateHubLifetimeContext(bool authenticated)
    {
        var hubContext = new Mock<HubCallerContext>();
        if (authenticated)
        {
            hubContext.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, "test")));
        }
        var mockHub = new Mock<object>().Object;
        return new HubLifetimeContext(hubContext.Object, _serviceProviderMock.Object, mockHub);
    }
}
