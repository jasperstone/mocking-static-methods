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
            public TestState SimpleStateChecker => this;
        }

        [Fact]
        public async Task IsEnabledAsync_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var permissions = new[] { "Permission1" };
            var requiresAll = true;
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), permissions, requiresAll);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(It.IsAny<string>()))
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
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPermissionChecker)), Times.Once);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync("Permission1"), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_WithMultiplePermissions_RequiresAllTrue_ReturnsAllGranted()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var requiresAll = true;
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), permissions, requiresAll);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var grantResult = new PermissionGrantResultBatch(
                new System.Collections.Generic.Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Granted }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
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
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPermissionChecker)), Times.Once);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_WithMultiplePermissions_RequiresAllFalse_ReturnsAnyGranted()
        {
            // Arrange
            var permissions = new[] { "Permission1", "Permission2" };
            var requiresAll = false;
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), permissions, requiresAll);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var grantResult = new PermissionGrantResultBatch(
                new System.Collections.Generic.Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Undefined }
                });

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
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
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPermissionChecker)), Times.Once);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissions), Times.Once);
        }
    }

    // Dummy implementations for missing types to make the test compile and run
    public interface IPermissionChecker
    {
        Task<bool> IsGrantedAsync(string permissionName);
        Task<PermissionGrantResultBatch> IsGrantedAsync(string[] permissionNames);
    }

    public class PermissionGrantResultBatch
    {
        public System.Collections.Generic.Dictionary<string, PermissionGrantResult> Result { get; }
        public bool AllGranted => Result.Values.All(r => r == PermissionGrantResult.Granted);

        public PermissionGrantResultBatch(System.Collections.Generic.Dictionary<string, PermissionGrantResult> result)
        {
            Result = result;
        }
    }

    public enum PermissionGrantResult
    {
        Undefined,
        Granted,
        Prohibited
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
