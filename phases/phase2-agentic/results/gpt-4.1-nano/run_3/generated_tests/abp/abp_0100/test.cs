using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;

namespace Volo.Abp.Authorization.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        private class DummyState : IHasSimpleStateCheckers<DummyState>
        {
            public bool RequiresAll => true;
            public string[] Permissions => new[] { "Permission1" };
        }

        private class DummyPermissionChecker : IPermissionChecker
        {
            public Func<string, Task<bool>> IsGrantedAsyncFunc { get; set; }
            public Func<string[], Task<PermissionGrantResult>> IsGrantedAsyncArrayFunc { get; set; }

            public Task<bool> IsGrantedAsync(string permissionName)
            {
                return IsGrantedAsyncFunc(permissionName);
            }

            public Task<PermissionGrantResult> IsGrantedAsync(string[] permissionNames)
            {
                return IsGrantedAsyncArrayFunc(permissionNames);
            }
        }

        [Fact]
        public async Task IsEnabledAsync_Should_Call_GetRequiredService_And_Return_True_When_PermissionGranted()
        {
            // Arrange
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            permissionCheckerMock.Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new PermissionGrantResult
                {
                    Result = new[] { (Key: "Permission1", Value: PermissionGrantResult.Granted) },
                    AllGranted = true
                });

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => permissionCheckerMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>
            {
                Permissions = new[] { "Permission1" },
                RequiresAll = true
            };

            var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);
            var context = new SimpleStateCheckerContext<DummyState>
            {
                ServiceProvider = serviceProvider
            };

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
            permissionCheckerMock.Verify(pc => pc.IsGrantedAsync("Permission1"), Times.Once);
        }
    }
}
