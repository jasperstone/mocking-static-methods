using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        [Fact]
        public async Task IsEnabledAsync_RequiresAll_ReturnsTrueIfAllPermissionsGranted()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>
            {
                RequiresAll = true,
                Permissions = new[] { "Permission1", "Permission2" }
            };

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);

            permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(new PermissionGrantResult
            {
                AllGranted = true,
                Result = new Dictionary<string, PermissionGrantResult>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Granted }
                }
            });

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_RequiresAll_ReturnsFalseIfNotAllPermissionsGranted()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>
            {
                RequiresAll = true,
                Permissions = new[] { "Permission1", "Permission2" }
            };

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);

            permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(new PermissionGrantResult
            {
                AllGranted = false,
                Result = new Dictionary<string, PermissionGrantResult>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.NotGranted }
                }
            });

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_DoesNotRequireAll_ReturnsTrueIfAnyPermissionGranted()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>
            {
                RequiresAll = false,
                Permissions = new[] { "Permission1", "Permission2" }
            };

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);

            permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(new PermissionGrantResult
            {
                AllGranted = false,
                Result = new Dictionary<string, PermissionGrantResult>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.NotGranted }
                }
            });

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_DoesNotRequireAll_ReturnsFalseIfNoPermissionsGranted()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>
            {
                RequiresAll = false,
                Permissions = new[] { "Permission1", "Permission2" }
            };

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);

            permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(new PermissionGrantResult
            {
                AllGranted = false,
                Result = new Dictionary<string, PermissionGrantResult>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Permission1", PermissionGrantResult.NotGranted },
                    { "Permission2", PermissionGrantResult.NotGranted }
                }
            });

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        private class TestState : IHasSimpleStateCheckers<TestState>
        {
            public void AddSimpleStateChecker(ISimpleStateChecker<TestState> checker)
            {
                throw new NotImplementedException();
            }
        }
    }
}
