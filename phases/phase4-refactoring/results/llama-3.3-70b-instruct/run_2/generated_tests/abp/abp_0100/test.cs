using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        [Fact]
        public async Task IsEnabledAsync_PermissionGranted_ReturnsTrue()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string>())).ReturnsAsync(new PermissionGrantResult(true));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPermissionChecker))).Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<MyState>(new MyState(), serviceProviderMock.Object);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>(new MyState(), new[] { "TestPermission" });
            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_PermissionNotGranted_ReturnsFalse()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string>())).ReturnsAsync(new PermissionGrantResult(false));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPermissionChecker))).Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<MyState>(new MyState(), serviceProviderMock.Object);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>(new MyState(), new[] { "TestPermission" });
            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }
    }

    public class MyState : IHasSimpleStateCheckers<MyState>
    {
        public List<ISimpleStateChecker<MyState>> StateCheckers { get; set; } = new List<ISimpleStateChecker<MyState>>();
    }
}
