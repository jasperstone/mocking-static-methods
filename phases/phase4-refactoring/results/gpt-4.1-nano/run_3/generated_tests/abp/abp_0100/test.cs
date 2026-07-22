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
            public List<string> Checkers { get; set; } = new List<string>();
            public IEnumerable<string> StateCheckers => Checkers;
        }

        [Fact]
        public async Task IsEnabledAsync_SinglePermission_CallsGetRequiredServiceAndReturnsResult()
        {
            // Arrange
            var permissionName = "TestPermission";

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>
            {
                Permissions = new[] { permissionName },
                RequiresAll = false
            };

            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(permissionName))
                .ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<DummyState>
            {
                ServiceProvider = serviceProviderMock.Object
            };

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync(permissionName), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IPermissionChecker>(), Times.Once);
        }
    }
}
