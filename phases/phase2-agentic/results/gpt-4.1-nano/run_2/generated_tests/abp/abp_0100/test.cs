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
            public List<string> Checkers { get; set; } = new List<string>();
        }

        private class DummyPermissionChecker : IPermissionChecker
        {
            public Func<string, Task<bool>> IsGrantedAsyncFunc { get; set; }
            public Func<string[], Task<PermissionGrantResult>> IsGrantedAsyncMultipleFunc { get; set; }

            public Task<bool> IsGrantedAsync(string permissionName)
            {
                return IsGrantedAsyncFunc != null ? IsGrantedAsyncFunc(permissionName) : Task.FromResult(false);
            }

            public Task<PermissionGrantResult> IsGrantedAsync(string[] permissionNames)
            {
                return IsGrantedAsyncMultipleFunc != null ? IsGrantedAsyncMultipleFunc(permissionNames) : Task.FromResult(new PermissionGrantResult(false));
            }
        }

        private class DummyGrantResult : PermissionGrantResult
        {
            public bool AllGranted { get; set; }
            public Dictionary<string, PermissionGrantResult> Result { get; set; }

            public DummyGrantResult(bool allGranted, Dictionary<string, PermissionGrantResult> result)
            {
                AllGranted = allGranted;
                Result = result;
            }
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Call_GetRequiredService()
        {
            // Arrange
            var permissionName = "TestPermission";

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>
            {
                Permissions = new[] { permissionName },
                RequiresAll = true
            };

            var serviceCollection = new ServiceCollection();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            serviceCollection.AddSingleton(permissionCheckerMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var context = new SimpleStateCheckerContext<DummyState>
            {
                ServiceProvider = serviceProvider
            };

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissionName), Times.Once);
            Assert.True(result);
        }
    }
}
