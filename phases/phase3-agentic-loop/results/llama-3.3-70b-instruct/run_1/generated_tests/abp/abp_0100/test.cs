using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        [Fact]
        public async Task IsEnabledAsync_RequiresAll_ReturnsTrue()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>(new MyState(), new[] { "Permission1" }, true);
            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);
            var context = new SimpleStateCheckerContext<MyState>(serviceProviderMock.Object);

            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string>())).ReturnsAsync(true);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_RequiresAll_ReturnsFalse()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>(new MyState(), new[] { "Permission1" }, true);
            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);
            var context = new SimpleStateCheckerContext<MyState>(serviceProviderMock.Object);

            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string>())).ReturnsAsync(false);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_DoesNotRequireAll_ReturnsTrue()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>(new MyState(), new[] { "Permission1" }, false);
            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);
            var context = new SimpleStateCheckerContext<MyState>(serviceProviderMock.Object);

            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string>())).ReturnsAsync(true);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_DoesNotRequireAll_ReturnsFalse()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>(new MyState(), new[] { "Permission1" }, false);
            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);
            var context = new SimpleStateCheckerContext<MyState>(serviceProviderMock.Object);

            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string>())).ReturnsAsync(false);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        private class MyState : IHasSimpleStateCheckers<MyState>
        {
            public ISimpleStateCheckerCollection<MyState> StateCheckers { get; } = new SimpleStateCheckerCollection<MyState>();
        }
    }
}
