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
        [Fact]
        public async Task IsEnabledAsync_ShouldReturnTrue_WhenSinglePermissionIsGranted()
        {
            // Arrange
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(x => x.IsGrantedAsync(It.IsAny<string>())).ReturnsAsync(PermissionGrantResult.Granted);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<object>(serviceProvider.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<object>(new object(), new[] { "Permission1" });
            var checker = new RequirePermissionsSimpleStateChecker<object>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnTrue_WhenAllPermissionsAreGranted()
        {
            // Arrange
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                { "Permission1", PermissionGrantResult.Granted },
                { "Permission2", PermissionGrantResult.Granted }
            }));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<object>(serviceProvider.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<object>(new object(), new[] { "Permission1", "Permission2" }, true);
            var checker = new RequirePermissionsSimpleStateChecker<object>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnFalse_WhenNotAllPermissionsAreGranted()
        {
            // Arrange
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                { "Permission1", PermissionGrantResult.Granted },
                { "Permission2", PermissionGrantResult.Prohibited }
            }));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<object>(serviceProvider.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<object>(new object(), new[] { "Permission1", "Permission2" }, true);
            var checker = new RequirePermissionsSimpleStateChecker<object>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }
    }
}
