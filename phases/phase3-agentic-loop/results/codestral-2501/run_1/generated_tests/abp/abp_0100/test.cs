using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Permissions.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        [Fact]
        public async Task IsEnabledAsync_ShouldCallGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" });
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            await checker.IsEnabledAsync(context);

            // Assert
            serviceProviderMock.Verify(x => x.GetRequiredService<IPermissionChecker>(), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnTrue_WhenSinglePermissionGranted()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" });
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnFalse_WhenSinglePermissionNotGranted()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" });
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnTrue_WhenAllPermissionsGranted()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new MultiplePermissionGrantResult
                {
                    Result = new Dictionary<string, PermissionGrantResult>
                    {
                        { "Permission1", PermissionGrantResult.Granted },
                        { "Permission2", PermissionGrantResult.Granted }
                    }
                });
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1", "Permission2" }, true);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnFalse_WhenNotAllPermissionsGranted()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new MultiplePermissionGrantResult
                {
                    Result = new Dictionary<string, PermissionGrantResult>
                    {
                        { "Permission1", PermissionGrantResult.Granted },
                        { "Permission2", PermissionGrantResult.Prohibited }
                    }
                });
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1", "Permission2" }, true);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnTrue_WhenAnyPermissionGranted()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new MultiplePermissionGrantResult
                {
                    Result = new Dictionary<string, PermissionGrantResult>
                    {
                        { "Permission1", PermissionGrantResult.Granted },
                        { "Permission2", PermissionGrantResult.Prohibited }
                    }
                });
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1", "Permission2" }, false);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnFalse_WhenNoPermissionsGranted()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new MultiplePermissionGrantResult
                {
                    Result = new Dictionary<string, PermissionGrantResult>
                    {
                        { "Permission1", PermissionGrantResult.Prohibited },
                        { "Permission2", PermissionGrantResult.Prohibited }
                    }
                });
            serviceProviderMock
                .Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1", "Permission2" }, false);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        public class TestState : IHasSimpleStateCheckers<TestState>
        {
            public List<ISimpleStateChecker<TestState>> StateCheckers { get; set; }

            public List<ISimpleStateChecker<TestState>> GetStateCheckers()
            {
                return StateCheckers ?? new List<ISimpleStateChecker<TestState>>();
            }
        }
    }
}
