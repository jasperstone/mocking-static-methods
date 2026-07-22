using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;

namespace Volo.Abp.Authorization.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        private class DummyState : IHasSimpleStateCheckers<DummyState>
        {
            public IEnumerable<string> StateCheckers => throw new NotImplementedException();
            public List<string> Checkers { get; set; } = new List<string>();
        }

        private class GrantResult
        {
            public bool AllGranted { get; set; }
            public Dictionary<string, PermissionGrantResult> Result { get; set; }
        }

        [Fact]
        public async Task IsEnabledAsync_SinglePermission_ReturnsExpected()
        {
            // Arrange
            var permissionName = "Permission1";

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissionName))
                .ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>
            {
                Permissions = new[] { permissionName },
                RequiresAll = false
            };

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);
            var context = new SimpleStateCheckerContext<DummyState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissionName), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAll_True_ReturnsExpected()
        {
            // Arrange
            var permissions = new[] { "Perm1", "Perm2" };
            var grantResult = new GrantResult
            {
                AllGranted = true,
                Result = new Dictionary<string, PermissionGrantResult>
                {
                    { "Perm1", PermissionGrantResult.Granted },
                    { "Perm2", PermissionGrantResult.Granted }
                }
            };

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>
            {
                Permissions = permissions,
                RequiresAll = true
            };

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);
            var context = new SimpleStateCheckerContext<DummyState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_NotAllRequired_ReturnsExpected()
        {
            // Arrange
            var permissions = new[] { "Perm1", "Perm2" };
            var grantResult = new GrantResult
            {
                AllGranted = false,
                Result = new Dictionary<string, PermissionGrantResult>
                {
                    { "Perm1", PermissionGrantResult.Granted },
                    { "Perm2", PermissionGrantResult.Denied }
                }
            };

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>
            {
                Permissions = permissions,
                RequiresAll = false
            };

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);
            var context = new SimpleStateCheckerContext<DummyState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }
    }
}
