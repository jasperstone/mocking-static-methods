using System;
using System.Collections.Generic;
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
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x =>
                    x.IsGrantedAsync(It.IsAny<string[]>) == Task.FromResult(new PermissionGrantResult { AllGranted = true })))
                .BuildServiceProvider();

            var context = new SimpleStateCheckerContext<TestState>(serviceProvider, new TestState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" }, true);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_RequiresAll_ReturnsFalse()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x =>
                    x.IsGrantedAsync(It.IsAny<string[]>) == Task.FromResult(new PermissionGrantResult { AllGranted = false })))
                .BuildServiceProvider();

            var context = new SimpleStateCheckerContext<TestState>(serviceProvider, new TestState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" }, true);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_DoesNotRequireAll_ReturnsTrue()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x =>
                    x.IsGrantedAsync(It.IsAny<string[]>) == Task.FromResult(new PermissionGrantResult { Result = new Dictionary<string, PermissionGrantResult> { { "Permission1", new PermissionGrantResult { AllGranted = true } } } })))
                .BuildServiceProvider();

            var context = new SimpleStateCheckerContext<TestState>(serviceProvider, new TestState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" }, false);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_DoesNotRequireAll_ReturnsFalse()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x =>
                    x.IsGrantedAsync(It.IsAny<string[]>) == Task.FromResult(new PermissionGrantResult { Result = new Dictionary<string, PermissionGrantResult> { { "Permission1", new PermissionGrantResult { AllGranted = false } } } })))
                .BuildServiceProvider();

            var context = new SimpleStateCheckerContext<TestState>(serviceProvider, new TestState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" }, false);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }
    }

    public class TestState : IHasSimpleStateCheckers<TestState>
    {
        public List<ISimpleStateChecker<TestState>> StateCheckers { get; set; } = new List<ISimpleStateChecker<TestState>>();
    }
}
