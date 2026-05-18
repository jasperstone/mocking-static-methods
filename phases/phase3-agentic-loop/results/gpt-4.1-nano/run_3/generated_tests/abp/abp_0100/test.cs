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
        private class TestState : IHasSimpleStateCheckers<TestState>
        {
            public List<string> Checkers { get; } = new List<string>();
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Call_GetRequiredService_And_Return_Expected_Result_For_Single_Permission()
        {
            // Arrange
            var permissionName = "TestPermission";

            var mockPermissionChecker = new Mock<IPermissionChecker>();
            mockPermissionChecker
                .Setup(pc => pc.IsGrantedAsync(permissionName))
                .ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
                .Returns(mockPermissionChecker.Object);

            var state = new TestState();

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
                state,
                new[] { permissionName },
                false
            );

            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            var context = new SimpleStateCheckerContext<TestState>(
                serviceProviderMock.Object,
                state
            );

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            mockPermissionChecker.Verify(pc => pc.IsGrantedAsync(permissionName), Times.Once);
        }
    }
}
