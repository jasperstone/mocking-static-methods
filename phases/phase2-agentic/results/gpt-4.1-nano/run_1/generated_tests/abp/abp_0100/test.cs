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
            public List<string> Checkers { get; } = new List<string>();
        }

        private class DummyPermissionChecker : IPermissionChecker
        {
            public Func<string, Task<bool>> IsGrantedAsyncFunc { get; set; }
            public Func<string[], Task<PermissionGrantResult>> IsGrantedAsyncArrayFunc { get; set; }

            public Task<bool> IsGrantedAsync(string permissionName)
            {
                return IsGrantedAsyncFunc != null ? IsGrantedAsyncFunc(permissionName) : Task.FromResult(false);
            }

            public Task<PermissionGrantResult> IsGrantedAsync(string[] permissionNames)
            {
                return IsGrantedAsyncArrayFunc != null ? IsGrantedAsyncArrayFunc(permissionNames) : Task.FromResult(new PermissionGrantResult(false));
            }
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Call_GetRequiredService_And_Return_True_When_PermissionGranted()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
                new DummyState(),
                new[] { "Permission1" },
                true
            );

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            var context = new SimpleStateCheckerContext<DummyState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync("Permission1"), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Handle_Multiple_Permissions_With_AllRequirement()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new PermissionGrantResult(true));
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
                new DummyState(),
                new[] { "Permission1", "Permission2" },
                true
            );

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            var context = new SimpleStateCheckerContext<DummyState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(It.Is<string[]>(p => p.SequenceEqual(new[] { "Permission1", "Permission2" }))), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Handle_Multiple_Permissions_With_AnyRequirement()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResultType>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Denied }
                }));
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
                new DummyState(),
                new[] { "Permission1", "Permission2" },
                false
            );

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            var context = new SimpleStateCheckerContext<DummyState>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(It.Is<string[]>(p => p.SequenceEqual(new[] { "Permission1", "Permission2" }))), Times.Once);
        }
    }
}
