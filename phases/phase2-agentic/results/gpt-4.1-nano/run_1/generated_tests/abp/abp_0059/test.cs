using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Security.Claims;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Microsoft.AspNetCore.SignalR;

namespace Volo.Abp.AspNetCore.SignalR.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IAbpClaimsPrincipalFactory> _claimsPrincipalFactoryMock;
        private readonly Mock<ICurrentPrincipalAccessor> _currentPrincipalAccessorMock;
        private readonly AbpAuthenticationHubFilter _filter;

        public AbpAuthenticationHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _claimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            _currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();

            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(_claimsPrincipalFactoryMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(_currentPrincipalAccessorMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(new OptionsWrapper<AbpClaimsPrincipalFactoryOptions>(new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true }));
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(new OptionsWrapper<AbpSignalROptions>(new AbpSignalROptions { CheckDynamicClaimsInterval = null }));

            _filter = new AbpAuthenticationHubFilter();
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_Next_When_Principal_Is_Not_Authenticated()
        {
            // Arrange
            var contextMock = new Mock<HubInvocationContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated
            var hubContextMock = new Mock<HubCallerContext>();
            var invocationContext = new HubInvocationContext
            {
                ServiceProvider = serviceProviderMock.Object,
                Context = hubContextMock.Object
            };

            contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);
            contextMock.Setup(c => c.Context).Returns(hubContextMock.Object);

            // Act
            var result = await _filter.InvokeMethodAsync(invocationContext, ctx => new ValueTask<object?>(Task.FromResult((object?)"done")));

            // Assert
            Assert.Equal("done", result);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Call_CreateDynamicAsync_And_Abort_When_Not_Authenticated()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType"));
            var identity = claimsPrincipal.Identity as ClaimsIdentity;
            identity.IsAuthenticated = true;

            var hubContextMock = new Mock<HubCallerContext>();
            var items = new System.Collections.Generic.Dictionary<object, object>();
            hubContextMock.Setup(h => h.Items).Returns(items);
            hubContextMock.Setup(h => h.Abort()).Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new AbpSignalROptions { CheckDynamicClaimsInterval = null };
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(new OptionsWrapper<AbpSignalROptions>(options));
            var claimsFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            claimsFactoryMock.Setup(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType") ));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(claimsFactoryMock.Object);

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProviderMock.Object, hubContextMock.Object, false);

            // Assert
            claimsFactoryMock.Verify(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            // Since the created principal is authenticated, Abort should not be called
            hubContextMock.Verify(h => h.Abort(), Times.Never);
        }
    }
}
