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
        [Fact]
        public async Task IsEnabledAsync_ShouldCallGetRequiredService()
        {
            // Arrange
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" });
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            await checker.IsEnabledAsync(context);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IPermissionChecker>(), Times.Once);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnTrue_WhenSinglePermissionGranted()
        {
            // Arrange
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(pc => pc.IsGrantedAsync("Permission1")).ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

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
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(pc => pc.IsGrantedAsync("Permission1")).ReturnsAsync(false);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

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
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(pc => pc.IsGrantedAsync(new[] { "Permission1", "Permission2" })).ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                { "Permission1", PermissionGrantResult.Granted },
                { "Permission2", PermissionGrantResult.Granted }
            }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

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
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(pc => pc.IsGrantedAsync(new[] { "Permission1", "Permission2" })).ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                { "Permission1", PermissionGrantResult.Granted },
                { "Permission2", PermissionGrantResult.Prohibited }
            }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

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
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(pc => pc.IsGrantedAsync(new[] { "Permission1", "Permission2" })).ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                { "Permission1", PermissionGrantResult.Prohibited },
                { "Permission2", PermissionGrantResult.Granted }
            }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

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
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(pc => pc.IsGrantedAsync(new[] { "Permission1", "Permission2" })).ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                { "Permission1", PermissionGrantResult.Prohibited },
                { "Permission2", PermissionGrantResult.Prohibited }
            }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1", "Permission2" }, false);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        public class TestState : IHasSimpleStateCheckers<TestState>
        {
            public IEnumerable<ISimpleStateChecker<TestState>> GetStateCheckers()
            {
                throw new NotImplementedException();
            }
        }
    }
}
