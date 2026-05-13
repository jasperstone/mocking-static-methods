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

namespace Volo.Abp.AspNetCore.SignalR.Authentication.Tests
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
                .Returns(new OptionsWrapper<AbpClaimsPrincipalFactoryOptions>(new AbpClaimsPrincipalFactoryOptions()));

            _filter = new AbpAuthenticationHubFilter();
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_Next_And_Change_Principal()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType"));
            var hubContextMock = new Mock<HubCallerContext>();
            var invocationContext = new HubInvocationContext(
                Mock.Of<HubConnectionContext>(), 
                "MethodName", 
                new object[0], 
                new HubInvocationMessage(), 
                new HubCallerContext(), 
                _serviceProviderMock.Object);

            var invocationContextMock = new Mock<HubInvocationContext>();
            invocationContextMock.SetupGet(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);
            invocationContextMock.SetupGet(c => c.Context).Returns(new HubCallerContext());
            invocationContextMock.SetupGet(c => c.Context.User).Returns(claimsPrincipal);
            invocationContextMock.SetupGet(c => c.InvocationId).Returns(Guid.NewGuid().ToString());

            var nextCalled = false;
            Func<HubInvocationContext, ValueTask<object?>> next = ctx =>
            {
                nextCalled = true;
                return new ValueTask<object?>(null);
            };

            // Act
            await _filter.InvokeMethodAsync(invocationContextMock.Object, next);

            // Assert
            Assert.True(nextCalled);
            _currentPrincipalAccessorMock.Verify(cpa => cpa.Change(claimsPrincipal), Times.Once);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Abort_When_ClaimsNotAuthenticated()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var options = new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true };
            var optionsWrapper = new OptionsWrapper<AbpClaimsPrincipalFactoryOptions>(options);
            var serviceProviderMock = new Mock<IServiceProvider>();
            var hubCallerContextMock = new Mock<HubCallerContext>();
            var claimsFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(optionsWrapper);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(claimsFactoryMock.Object);
            hubCallerContextMock.SetupGet(c => c.Items).Returns(new System.Collections.Generic.Dictionary<object, object>());
            hubCallerContextMock.Setup(c => c.Abort());

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProviderMock.Object, hubCallerContextMock.Object, false);

            // Assert
            hubCallerContextMock.Verify(c => c.Abort(), Times.Once);
        }
    }
}
