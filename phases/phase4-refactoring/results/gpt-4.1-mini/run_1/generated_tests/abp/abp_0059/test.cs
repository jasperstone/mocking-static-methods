using System;
using System.Collections;
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

namespace Volo.Abp.AspNetCore.SignalR.Authentication.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        private class DummyHub : Hub
        {
            public Task DummyMethod() => Task.CompletedTask;
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_GetRequiredService_And_InvokeNext()
        {
            // Arrange
            var mockCurrentPrincipalAccessor = new Mock<ICurrentPrincipalAccessor>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            var mockSignalROptions = new Mock<IOptions<AbpSignalROptions>>();
            var mockClaimsPrincipalFactory = new Mock<IAbpClaimsPrincipalFactory>();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType"));
            var items = new Dictionary<object, object>();
            var mockHubCallerContext = new Mock<HubCallerContext>();
            mockHubCallerContext.SetupGet(c => c.User).Returns(claimsPrincipal);
            mockHubCallerContext.SetupGet(c => c.Items).Returns(items);

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ICurrentPrincipalAccessor))).Returns(mockCurrentPrincipalAccessor.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<AbpClaimsPrincipalFactoryOptions>))).Returns(mockOptions.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<AbpSignalROptions>))).Returns(mockSignalROptions.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IAbpClaimsPrincipalFactory))).Returns(mockClaimsPrincipalFactory.Object);

            mockOptions.Setup(o => o.Value).Returns(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false });
            mockSignalROptions.Setup(o => o.Value).Returns(new AbpSignalROptions { CheckDynamicClaimsInterval = null });
            mockClaimsPrincipalFactory.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(claimsPrincipal);

            var methodInfo = typeof(DummyHub).GetMethod(nameof(DummyHub.DummyMethod))!;
            var invocationContext = new HubInvocationContext(
                mockHubCallerContext.Object,
                mockServiceProvider.Object,
                new DummyHub(),
                methodInfo,
                Array.Empty<object?>());

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            async ValueTask<object?> Next(HubInvocationContext ctx)
            {
                nextCalled = true;
                return "result";
            }

            mockCurrentPrincipalAccessor.Setup(cpa => cpa.Change(It.IsAny<ClaimsPrincipal>())).Returns(Mock.Of<IDisposable>());

            // Act
            var result = await filter.InvokeMethodAsync(invocationContext, Next);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal("result", result);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ICurrentPrincipalAccessor)), Times.AtLeastOnce);
        }
    }
}
