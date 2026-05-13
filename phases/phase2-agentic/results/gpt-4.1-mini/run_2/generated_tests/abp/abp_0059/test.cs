using System;
using System.Collections.Generic;
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
        [Fact]
        public async Task InvokeMethodAsync_Should_Call_Next_And_Use_CurrentPrincipalAccessor_Change()
        {
            // Arrange
            var mockCurrentPrincipalAccessor = new Mock<ICurrentPrincipalAccessor>();
            var mockDisposable = new Mock<IDisposable>();
            mockCurrentPrincipalAccessor.Setup(x => x.Change(It.IsAny<ClaimsPrincipal>())).Returns(mockDisposable.Object);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuthType") { });

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetRequiredService<ICurrentPrincipalAccessor>()).Returns(mockCurrentPrincipalAccessor.Object);
            mockServiceProvider.Setup(x => x.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false }));

            var mockHubCallerContext = new Mock<HubCallerContext>();
            mockHubCallerContext.SetupGet(c => c.User).Returns(claimsPrincipal);

            var invocationContext = new HubInvocationContext(
                mockHubCallerContext.Object,
                mockServiceProvider.Object,
                "TestMethod",
                new object[0]);

            var filter = new AbpAuthenticationHubFilter();

            var nextCalled = false;
            ValueTask<object?> Next(HubInvocationContext ctx)
            {
                nextCalled = true;
                return new ValueTask<object?>(Task.FromResult<object?>("result"));
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
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Abort_If_Identity_Is_Not_Authenticated_After_CreateDynamicAsync()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity("TestAuthType") { };
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var abpClaimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            var unauthenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity() { });
            abpClaimsPrincipalFactoryMock.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(unauthenticatedPrincipal);

            var abpClaimsPrincipalFactoryOptions = Options.Create(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
            var abpSignalROptions = Options.Create(new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromSeconds(1) });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>()).Returns(abpClaimsPrincipalFactoryOptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>()).Returns(abpSignalROptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>()).Returns(abpClaimsPrincipalFactoryMock.Object);

            var hubCallerContextMock = new Mock<HubCallerContext>();
            var items = new Dictionary<string, object>();
            hubCallerContextMock.SetupGet(c => c.Items).Returns(items);
            hubCallerContextMock.Setup(c => c.Abort());

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProviderMock.Object, hubCallerContextMock.Object, false);

            // Assert
            hubCallerContextMock.Verify(c => c.Abort(), Times.Once);
        }
    }
}
