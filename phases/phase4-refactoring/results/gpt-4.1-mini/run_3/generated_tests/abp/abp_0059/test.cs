using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security.Claims;
using Xunit;

public class AbpAuthenticationHubFilterTests
{
    private class TestHub : Hub
    {
        public Task<string> TestMethod() => Task.FromResult("test");
    }

    [Fact]
    public async Task InvokeMethodAsync_Should_Call_Next_And_Use_CurrentPrincipalAccessor_Change()
    {
        // Arrange
        var mockCurrentPrincipalAccessor = new Mock<ICurrentPrincipalAccessor>();
        var mockDisposable = new Mock<IDisposable>();
        mockCurrentPrincipalAccessor.Setup(x => x.Change(It.IsAny<ClaimsPrincipal>())).Returns(mockDisposable.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(ICurrentPrincipalAccessor))).Returns(mockCurrentPrincipalAccessor.Object);
        mockServiceProvider.Setup(x => x.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>>)))
            .Returns(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false }));

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType"));
        var mockHubCallerContext = new Mock<HubCallerContext>();
        mockHubCallerContext.SetupGet(c => c.User).Returns(claimsPrincipal);
        mockHubCallerContext.SetupGet(c => c.Items).Returns(new Dictionary<object, object>());

        var methodInfo = typeof(TestHub).GetMethod(nameof(TestHub.TestMethod))!;
        var invocationContext = new HubInvocationContext(
            mockHubCallerContext.Object,
            mockServiceProvider.Object,
            new TestHub(),
            methodInfo,
            Array.Empty<object?>());

        var filter = new AbpAuthenticationHubFilter();

        var nextCalled = false;
        async ValueTask<object?> Next(HubInvocationContext ctx)
        {
            nextCalled = true;
            return "result";
        }

        // Act
        var result = await filter.InvokeMethodAsync(invocationContext, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("result", result);
        mockCurrentPrincipalAccessor.Verify(x => x.Change(claimsPrincipal), Times.Once);
        mockDisposable.Verify(d => d.Dispose(), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_Should_Call_Next_And_Use_CurrentPrincipalAccessor_Change()
    {
        // Arrange
        var mockCurrentPrincipalAccessor = new Mock<ICurrentPrincipalAccessor>();
        var mockDisposable = new Mock<IDisposable>();
        mockCurrentPrincipalAccessor.Setup(x => x.Change(It.IsAny<ClaimsPrincipal>())).Returns(mockDisposable.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(ICurrentPrincipalAccessor))).Returns(mockCurrentPrincipalAccessor.Object);
        mockServiceProvider.Setup(x => x.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>>)))
            .Returns(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false }));

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType"));
        var mockHubCallerContext = new Mock<HubCallerContext>();
        mockHubCallerContext.SetupGet(c => c.User).Returns(claimsPrincipal);
        mockHubCallerContext.SetupGet(c => c.Items).Returns(new Dictionary<object, object>());

        var lifetimeContext = new HubLifetimeContext(
            mockHubCallerContext.Object,
            mockServiceProvider.Object,
            new TestHub());

        var filter = new AbpAuthenticationHubFilter();

        var nextCalled = false;
        Task Next(HubLifetimeContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await filter.OnConnectedAsync(lifetimeContext, Next);

        // Assert
        Assert.True(nextCalled);
        mockCurrentPrincipalAccessor.Verify(x => x.Change(claimsPrincipal), Times.Once);
        mockDisposable.Verify(d => d.Dispose(), Times.Once);
    }
}
