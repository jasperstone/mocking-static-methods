using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        [Fact]
        public async Task IsEnabledAsync_RequiresAll_ReturnsTrue()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<MyState>(serviceProviderMock.Object, new MyState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>
            {
                State = new MyState(),
                Permissions = new[] { "Permission1" },
                RequiresAll = true
            };

            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_RequiresAll_ReturnsFalse()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<MyState>(serviceProviderMock.Object, new MyState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>
            {
                State = new MyState(),
                Permissions = new[] { "Permission1" },
                RequiresAll = true
            };

            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_DoesNotRequireAll_ReturnsTrue()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<MyState>(serviceProviderMock.Object, new MyState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>
            {
                State = new MyState(),
                Permissions = new[] { "Permission1" },
                RequiresAll = false
            };

            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_DoesNotRequireAll_ReturnsFalse()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<MyState>(serviceProviderMock.Object, new MyState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MyState>
            {
                State = new MyState(),
                Permissions = new[] { "Permission1" },
                RequiresAll = false
            };

            var checker = new RequirePermissionsSimpleStateChecker<MyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }
    }

    public class MyState : IHasSimpleStateCheckers<MyState>
    {
        public ISimpleStateChecker<MyState>[] StateCheckers { get; set; }
    }
}
