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
            public IReadOnlyList<ISimpleStateChecker<TestState>> SimpleStateCheckers => Array.Empty<ISimpleStateChecker<TestState>>();
        }

        [Fact]
        public async Task IsEnabledAsync_SinglePermission_Granted_ReturnsTrue()
        {
            // Arrange
            var permissionName = "Permission1";
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { permissionName }, requiresAll: true);

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(permissionName)).ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPermissionChecker))).Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, new TestState());

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

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
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), permissions, requiresAll: true);

            var grantResult = new PermissionGrantResult(
                new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Granted }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(permissions)).ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPermissionChecker))).Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, new TestState());

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

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
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), permissions, requiresAll: true);

            var grantResult = new PermissionGrantResult(
                new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Undefined }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(permissions)).ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPermissionChecker))).Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, new TestState());

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

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
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), permissions, requiresAll: false);

            var grantResult = new PermissionGrantResult(
                new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Denied },
                    { "Permission2", PermissionGrantResult.Granted }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(permissions)).ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPermissionChecker))).Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, new TestState());

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

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
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), permissions, requiresAll: false);

            var grantResult = new PermissionGrantResult(
                new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Denied },
                    { "Permission2", PermissionGrantResult.Denied }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(permissions)).ReturnsAsync(grantResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPermissionChecker))).Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, new TestState());

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }
    }

    // Minimal stubs for dependencies

    public interface IPermissionChecker
    {
        Task<bool> IsGrantedAsync(string permissionName);
        Task<PermissionGrantResult> IsGrantedAsync(string[] permissionNames);
    }

    public class PermissionGrantResult
    {
        public static readonly PermissionGrantResult Granted = new PermissionGrantResult(true);
        public static readonly PermissionGrantResult Denied = new PermissionGrantResult(false);
        public static readonly PermissionGrantResult Undefined = new PermissionGrantResult(false);

        public bool AllGranted { get; }
        public IReadOnlyDictionary<string, PermissionGrantResult> Result { get; }

        public PermissionGrantResult(bool allGranted)
        {
            AllGranted = allGranted;
            Result = new Dictionary<string, PermissionGrantResult>();
        }

        public PermissionGrantResult(IReadOnlyDictionary<string, PermissionGrantResult> result)
        {
            Result = result;
            AllGranted = result.Values.All(r => r == Granted);
        }

        public override bool Equals(object obj)
        {
            if (obj is PermissionGrantResult other)
            {
                return AllGranted == other.AllGranted && Result.SequenceEqual(other.Result);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AllGranted, Result);
        }
    }

    public class SimpleStateCheckerContext<TState>
    {
        public IServiceProvider ServiceProvider { get; }
        public TState State { get; }

        public SimpleStateCheckerContext(IServiceProvider serviceProvider, TState state)
        {
            ServiceProvider = serviceProvider;
            State = state;
        }
    }
}
