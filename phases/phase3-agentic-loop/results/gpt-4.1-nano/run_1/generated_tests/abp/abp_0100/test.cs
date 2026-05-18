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
            public IEnumerable<ISimpleStateChecker<DummyState>> StateCheckers => throw new NotImplementedException();
        }

        private class DummyGrantResult
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

            var contextMock = new Mock<SimpleStateCheckerContext<DummyState>>();
            contextMock
                .Setup(c => c.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
                new DummyState(),
                new[] { permissionName },
                false
            );

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(contextMock.Object);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissionName), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAll_ReturnsExpected()
        {
            // Arrange
            var permissions = new[] { "Perm1", "Perm2" };
            var grantResults = new Dictionary<string, PermissionGrantResult>
            {
                { "Perm1", PermissionGrantResult.Granted },
                { "Perm2", PermissionGrantResult.Granted }
            };

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(new DummyGrantResult
                {
                    AllGranted = true,
                    Result = grantResults
                });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var contextMock = new Mock<SimpleStateCheckerContext<DummyState>>();
            contextMock
                .Setup(c => c.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
                new DummyState(),
                permissions,
                true
            );

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(contextMock.Object);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_Any_ReturnsExpected()
        {
            // Arrange
            var permissions = new[] { "Perm1", "Perm2" };
            var grantResults = new Dictionary<string, PermissionGrantResult>
            {
                { "Perm1", PermissionGrantResult.Denied },
                { "Perm2", PermissionGrantResult.Granted }
            };

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(new DummyGrantResult
                {
                    AllGranted = false,
                    Result = grantResults
                });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var contextMock = new Mock<SimpleStateCheckerContext<DummyState>>();
            contextMock
                .Setup(c => c.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
                new DummyState(),
                permissions,
                false
            );

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(contextMock.Object);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }
    }
}
