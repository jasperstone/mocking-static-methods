using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Permissions.Tests
{
    public class RequirePermissionsSimpleStateCheckerTests
    {
        [Fact]
        public async Task IsEnabledAsync_RequiresAll_ReturnsTrue()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x => x.IsGrantedAsync(It.IsAny<string[]>) == Task.FromResult(new PermissionGrantResult { AllGranted = true })))
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
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x => x.IsGrantedAsync(It.IsAny<string[]>) == Task.FromResult(new PermissionGrantResult { AllGranted = false })))
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
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x => x.IsGrantedAsync(It.IsAny<string[]>) == Task.FromResult(new PermissionGrantResult { Result = new Dictionary<string, PermissionGrantResult> { { "Permission1", new PermissionGrantResult { } } } })))
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
                .AddSingleton<IPermissionChecker>(Mock.Of<IPermissionChecker>(x => x.IsGrantedAsync(It.IsAny<string[]>) == Task.FromResult(new PermissionGrantResult { Result = new Dictionary<string, PermissionGrantResult> { } })))
                .BuildServiceProvider();

            var context = new SimpleStateCheckerContext<TestState>(serviceProvider, new TestState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" }, false);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            var result = await checker.IsEnabledAsync(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEnabledAsync_GetRequiredService_CallsGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var permissionCheckerMock = new Mock<IPermissionChecker>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

            var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, new TestState());
            var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(new TestState(), new[] { "Permission1" }, true);
            var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

            // Act
            await checker.IsEnabledAsync(context);

            // Assert
            serviceProviderMock.Verify(x => x.GetRequiredService<IPermissionChecker>(), Times.Once);
        }
    }

    public class TestState : IHasSimpleStateCheckers<TestState>
    {
        public List<ISimpleStateChecker<TestState>> StateCheckers { get; set; }
    }
}
