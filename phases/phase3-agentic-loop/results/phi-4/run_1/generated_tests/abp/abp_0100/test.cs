using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Tests.Permissions
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        private class MockStateChecker : IHasSimpleStateCheckers<MockStateChecker>
        {
            public ISimpleStateChecker<MockStateChecker>[] SimpleStateCheckers { get; set; } = Array.Empty<ISimpleStateChecker<MockStateChecker>>();
        }

        [Fact]
        public async Task IsEnabledAsync_WhenSinglePermission_ReturnsCorrectResult()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockStateChecker>(new MockStateChecker(), new[] { "TestPermission" });
            var checker = new RequirePermissionsSimpleStateChecker<MockStateChecker>(model);

            var context = new SimpleStateCheckerContext<MockStateChecker>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(x => x.IsGrantedAsync("TestPermission"), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_WhenMultiplePermissions_AllGranted_ReturnsTrue()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new PermissionGrantResult(true, new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Granted }
                }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockStateChecker>(new MockStateChecker(), new[] { "Permission1", "Permission2" });
            var checker = new RequirePermissionsSimpleStateChecker<MockStateChecker>(model);

            var context = new SimpleStateCheckerContext<MockStateChecker>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(x => x.IsGrantedAsync(It.Is<IEnumerable<string>>(p => p.SequenceEqual(new[] { "Permission1", "Permission2" }))), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_WhenMultiplePermissions_SomeGranted_ReturnsTrue()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new PermissionGrantResult(false, new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.NotGranted }
                }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockStateChecker>(new MockStateChecker(), new[] { "Permission1", "Permission2" }, requiresAll: false);
            var checker = new RequirePermissionsSimpleStateChecker<MockStateChecker>(model);

            var context = new SimpleStateCheckerContext<MockStateChecker>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(x => x.IsGrantedAsync(It.Is<IEnumerable<string>>(p => p.SequenceEqual(new[] { "Permission1", "Permission2" }))), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_WhenMultiplePermissions_NoneGranted_ReturnsFalse()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new PermissionGrantResult(false, new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.NotGranted },
                    { "Permission2", PermissionGrantResult.NotGranted }
                }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockStateChecker>(new MockStateChecker(), new[] { "Permission1", "Permission2" });
            var checker = new RequirePermissionsSimpleStateChecker<MockStateChecker>(model);

            var context = new SimpleStateCheckerContext<MockStateChecker>(serviceProviderMock.Object);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
            permissionCheckerMock.Verify(x => x.IsGrantedAsync(It.Is<IEnumerable<string>>(p => p.SequenceEqual(new[] { "Permission1", "Permission2" }))), Times.Once);
        }
    }
}
