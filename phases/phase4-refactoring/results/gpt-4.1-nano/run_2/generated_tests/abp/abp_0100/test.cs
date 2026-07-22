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
            public List<DummyState> SimpleStates { get; } = new List<DummyState>();
            public IEnumerable<IHasSimpleStateCheckers<DummyState>> StateCheckers => throw new NotImplementedException();
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Call_GetRequiredService_And_Return_True_When_Permission_Granted()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            permissionCheckerMock
                .Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new PermissionGrantResult { AllGranted = true, Result = new Dictionary<string, PermissionGrantResult>() });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(permissionCheckerMock.Object);

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>
            {
                Permissions = new[] { "Permission1" },
                RequiresAll = false,
                State = new DummyState()
            };

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

            var context = new SimpleStateCheckerContext<DummyState>(
                serviceProviderMock.Object,
                new List<DummyState> { new DummyState() }
            );

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync("Permission1"), Times.Once);
        }
    }
}
