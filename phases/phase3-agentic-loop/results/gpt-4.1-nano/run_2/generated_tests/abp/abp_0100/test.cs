using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;

namespace Volo.Abp.Authorization.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        private class DummyState : IHasSimpleStateCheckers<DummyState>
        {
            public List<string> StateCheckers { get; } = new List<string>();
        }

        private class DummyGrantResult
        {
            public bool AllGranted { get; set; }
            public Dictionary<string, PermissionGrantResult> Result { get; set; }
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Call_GetRequiredService_And_Return_True_When_PermissionGranted_SinglePermission()
        {
            // Arrange
            var permissionName = "TestPermission";

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissionName))
                .ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
                new DummyState(),
                new[] { permissionName },
                true
            );

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            var context = new SimpleStateCheckerContext<DummyState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissionName), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Call_GetRequiredService_And_Return_Correct_Boolean_For_MultiplePermissions_When_RequiresAll_Is_False()
        {
            // Arrange
            var permissions = new[] { "Perm1", "Perm2" };
            var grantResults = new Dictionary<string, PermissionGrantResult>
            {
                { "Perm1", PermissionGrantResult.Granted },
                { "Perm2", PermissionGrantResult.Denied }
            };
            var grantResult = new DummyGrantResult
            {
                AllGranted = false,
                Result = grantResults
            };

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
                new DummyState(),
                permissions,
                false
            );

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);
            var context = new SimpleStateCheckerContext<DummyState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Return_RequiresAll_Property_When_MultiplePermissions()
        {
            // Arrange
            var permissions = new[] { "Perm1", "Perm2" };
            var grantResults = new Dictionary<string, PermissionGrantResult>
            {
                { "Perm1", PermissionGrantResult.Granted },
                { "Perm2", PermissionGrantResult.Granted }
            };
            var grantResult = new DummyGrantResult
            {
                AllGranted = true,
                Result = grantResults
            };

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
                new DummyState(),
                permissions,
                true // RequiresAll
            );

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);
            var context = new SimpleStateCheckerContext<DummyState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }
    }
}
