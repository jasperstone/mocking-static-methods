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
        public async Task IsEnabledAsync_ShouldReturnTrue_WhenSinglePermissionIsGranted()
        {
            // Arrange
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(x => x.IsGrantedAsync(It.IsAny<string>())).ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

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
        public async Task IsEnabledAsync_ShouldReturnTrue_WhenAllPermissionsAreGranted()
        {
            // Arrange
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                { "Permission1", PermissionGrantResult.Granted },
                { "Permission2", PermissionGrantResult.Granted }
            }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1", "Permission2" });
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

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

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1", "Permission2" });
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnTrue_WhenAnyPermissionIsGranted()
        {
            // Arrange
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                { "Permission1", PermissionGrantResult.Prohibited },
                { "Permission2", PermissionGrantResult.Granted }
            }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1", "Permission2" }, requiresAll: false);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ShouldReturnFalse_WhenNoPermissionsAreGranted()
        {
            // Arrange
            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(new PermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                { "Permission1", PermissionGrantResult.Prohibited },
                { "Permission2", PermissionGrantResult.Prohibited }
            }));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(mockPermissionChecker.Object);

            var context = new SimpleStateCheckerContext<TestState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1", "Permission2" }, requiresAll: false);
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
                return Enumerable.Empty<ISimpleStateChecker<TestState>>();
            }
        }
    }
}
