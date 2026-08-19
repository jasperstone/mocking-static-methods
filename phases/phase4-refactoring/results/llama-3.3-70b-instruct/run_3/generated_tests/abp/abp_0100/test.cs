using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        [Fact]
        public async Task IsEnabledAsync_PermissionGranted_ReturnsTrue()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x => x.IsGrantedAsync(It.IsAny<string>()) == Task.FromResult(PermissionGrantResult.Granted)))
                .BuildServiceProvider();

            var state = new MockState();
            var context = new SimpleStateCheckerContext<MockState>(state, serviceProvider);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(state, new[] { "Permission1" });
            var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_PermissionNotGranted_ReturnsFalse()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x => x.IsGrantedAsync(It.IsAny<string>()) == Task.FromResult(PermissionGrantResult.NotGranted)))
                .BuildServiceProvider();

            var state = new MockState();
            var context = new SimpleStateCheckerContext<MockState>(state, serviceProvider);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(state, new[] { "Permission1" });
            var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissionsAllGranted_ReturnsTrue()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x => x.IsGrantedAsync(It.IsAny<string[]>()) == Task.FromResult(new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.Granted }
                })))
                .BuildServiceProvider();

            var state = new MockState();
            var context = new SimpleStateCheckerContext<MockState>(state, serviceProvider);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(state, new[] { "Permission1", "Permission2" });
            var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_MultiplePermissionsNotAllGranted_ReturnsFalse()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x => x.IsGrantedAsync(It.IsAny<string[]>()) == Task.FromResult(new Dictionary<string, PermissionGrantResult>
                {
                    { "Permission1", PermissionGrantResult.Granted },
                    { "Permission2", PermissionGrantResult.NotGranted }
                })))
                .BuildServiceProvider();

            var state = new MockState();
            var context = new SimpleStateCheckerContext<MockState>(state, serviceProvider);
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(state, new[] { "Permission1", "Permission2" });
            var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        private class MockState : IHasSimpleStateCheckers<MockState>
        {
            public List<ISimpleStateChecker<MockState>> StateCheckers => new List<ISimpleStateChecker<MockState>>();
        }
    }
}
