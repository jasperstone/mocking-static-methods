using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Permissions.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        private class TestState : IHasSimpleStateCheckers<TestState>
        {
            public TestState Dummy => this;
        }

        [Fact]
        public async Task IsEnabledAsync_SinglePermission_Granted_ReturnsTrue()
        {
            // Arrange
            var permissionName = "Permission1";
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                new TestState(),
                new[] { permissionName },
                requiresAll: true);

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(permissionName))
                .ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(x => x.IsGrantedAsync(permissionName), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAll_TrueIfAllGranted()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                new TestState(),
                permissions,
                requiresAll: true);

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var multipleResult = new MultiplePermissionGrantResult();
            multipleResult.Result.Add("Permission1", PermissionGrantResult.Granted);
            multipleResult.Result.Add("Permission2", PermissionGrantResult.Granted);

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(permissions))
                .ReturnsAsync(multipleResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(x => x.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAll_FalseIfAnyNotGranted()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                new TestState(),
                permissions,
                requiresAll: true);

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var multipleResult = new MultiplePermissionGrantResult();
            multipleResult.Result.Add("Permission1", PermissionGrantResult.Granted);
            multipleResult.Result.Add("Permission2", PermissionGrantResult.Prohibited);

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(permissions))
                .ReturnsAsync(multipleResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
            permissionCheckerMock.Verify(x => x.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAny_TrueIfAnyGranted()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                new TestState(),
                permissions,
                requiresAll: false);

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var multipleResult = new MultiplePermissionGrantResult();
            multipleResult.Result.Add("Permission1", PermissionGrantResult.Prohibited);
            multipleResult.Result.Add("Permission2", PermissionGrantResult.Granted);

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(permissions))
                .ReturnsAsync(multipleResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(x => x.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAny_FalseIfNoneGranted()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                new TestState(),
                permissions,
                requiresAll: false);

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var multipleResult = new MultiplePermissionGrantResult();
            multipleResult.Result.Add("Permission1", PermissionGrantResult.Prohibited);
            multipleResult.Result.Add("Permission2", PermissionGrantResult.Prohibited);

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(permissions))
                .ReturnsAsync(multipleResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
            permissionCheckerMock.Verify(x => x.IsGrantedAsync(permissions), Times.Once);
        }
    }
}
