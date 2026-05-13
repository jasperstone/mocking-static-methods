using System;
using System.Collections.Generic;
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
            public IReadOnlyList<ISimpleStateChecker<TestState>> SimpleStateCheckers => new List<ISimpleStateChecker<TestState>>();
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
                .Setup(pc => pc.IsGrantedAsync(permissionName))
                .ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissionName), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAll_AllGranted_ReturnsTrue()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                new TestState(),
                permissions,
                requiresAll: true);

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var grantResult = new PermissionGrantResultBatch(
                new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Granted }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAll_NotAllGranted_ReturnsFalse()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                new TestState(),
                permissions,
                requiresAll: true);

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var grantResult = new PermissionGrantResultBatch(
                new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Prohibited }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAny_OneGranted_ReturnsTrue()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                new TestState(),
                permissions,
                requiresAll: false);

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var grantResult = new PermissionGrantResultBatch(
                new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Prohibited },
                    { "Permission2", PermissionGrantResult.Granted }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissions_RequiresAny_NoneGranted_ReturnsFalse()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                new TestState(),
                permissions,
                requiresAll: false);

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var grantResult = new PermissionGrantResultBatch(
                new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Prohibited },
                    { "Permission2", PermissionGrantResult.Prohibited }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissions))
                .ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IPermissionChecker)))
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }
    }

    // Supporting types for the tests

    public interface IPermissionChecker
    {
        Task<bool> IsGrantedAsync(string permissionName);
        Task<PermissionGrantResultBatch> IsGrantedAsync(string[] permissionNames);
    }

    public class PermissionGrantResultBatch
    {
        public IReadOnlyDictionary<string, PermissionGrantResult> Result { get; }

        public bool AllGranted => Result.Values.All(r => r == PermissionGrantResult.Granted);

        public PermissionGrantResultBatch(IReadOnlyDictionary<string, PermissionGrantResult> result)
        {
            Result = result;
        }
    }

    public enum PermissionGrantResult
    {
        Prohibited = 0,
        Granted = 1
    }

    public class SimpleStateCheckerContext<TState>
    {
        public IServiceProvider ServiceProvider { get; }

        public SimpleStateCheckerContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}
