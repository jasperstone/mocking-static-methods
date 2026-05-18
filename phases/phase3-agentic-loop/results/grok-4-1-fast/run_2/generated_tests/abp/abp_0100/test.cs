using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Permissions.Tests;

public class RequirePermissionsSimpleStateCheckerTests
{
    [Fact]
    public async Task IsEnabledAsync_SinglePermission_Granted_ReturnsTrue()
    {
        // Arrange
        var mockPermissionChecker = new Mock<IPermissionChecker>();
        mockPermissionChecker.Setup(x => x.IsGrantedAsync("Test.Permission")).ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton(mockPermissionChecker.Object);
        var serviceProvider = services.BuildServiceProvider();

        var testState = new TestState();
        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            testState,
            new[] { "Test.Permission" },
            requiresAll: true
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
        var context = new SimpleStateCheckerContext<TestState>(serviceProvider, testState);

        // Act
        var result = await checker.IsEnabledAsync(context);

        // Assert
        Assert.True(result);
        mockPermissionChecker.Verify(x => x.IsGrantedAsync("Test.Permission"), Times.Once);
    }

    [Fact]
    public async Task IsEnabledAsync_SinglePermission_NotGranted_ReturnsFalse()
    {
        // Arrange
        var mockPermissionChecker = new Mock<IPermissionChecker>();
        mockPermissionChecker.Setup(x => x.IsGrantedAsync("Test.Permission")).ReturnsAsync(false);

        var services = new ServiceCollection();
        services.AddSingleton(mockPermissionChecker.Object);
        var serviceProvider = services.BuildServiceProvider();

        var testState = new TestState();
        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            testState,
            new[] { "Test.Permission" },
            requiresAll: true
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
        var context = new SimpleStateCheckerContext<TestState>(serviceProvider, testState);

        // Act
        var result = await checker.IsEnabledAsync(context);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_MultiplePermissions_RequiresAllTrue_AllGranted_ReturnsTrue()
    {
        // Arrange
        var mockPermissionChecker = new Mock<IPermissionChecker>();
        mockPermissionChecker.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(
            new MultiplePermissionGrantResult
            {
                Result = new Dictionary<string, PermissionGrantResult>
                {
                    ["Perm1"] = PermissionGrantResult.Granted,
                    ["Perm2"] = PermissionGrantResult.Granted
                }
            });

        var services = new ServiceCollection();
        services.AddSingleton(mockPermissionChecker.Object);
        var serviceProvider = services.BuildServiceProvider();

        var testState = new TestState();
        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            testState,
            new[] { "Perm1", "Perm2" },
            requiresAll: true
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
        var context = new SimpleStateCheckerContext<TestState>(serviceProvider, testState);

        // Act
        var result = await checker.IsEnabledAsync(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsEnabledAsync_MultiplePermissions_RequiresAllFalse_AnyGranted_ReturnsTrue()
    {
        // Arrange
        var mockPermissionChecker = new Mock<IPermissionChecker>();
        mockPermissionChecker.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(
            new MultiplePermissionGrantResult
            {
                Result = new Dictionary<string, PermissionGrantResult>
                {
                    ["Perm1"] = PermissionGrantResult.Denied,
                    ["Perm2"] = PermissionGrantResult.Granted
                }
            });

        var services = new ServiceCollection();
        services.AddSingleton(mockPermissionChecker.Object);
        var serviceProvider = services.BuildServiceProvider();

        var testState = new TestState();
        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            testState,
            new[] { "Perm1", "Perm2" },
            requiresAll: false
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
        var context = new SimpleStateCheckerContext<TestState>(serviceProvider, testState);

        // Act
        var result = await checker.IsEnabledAsync(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Properties_ReturnExpectedValues()
    {
        // Arrange
        var testState = new TestState();
        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            testState,
            new[] { "Perm1", "Perm2" },
            requiresAll: false
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act & Assert
        Assert.False(checker.RequiresAll);
        Assert.Equal(new[] { "Perm1", "Perm2" }, checker.PermissionNames);
    }
}

public class TestState : IHasSimpleStateCheckers<TestState>
{
    public List<ISimpleStateChecker<TestState>> StateCheckers { get; set; } = new();
}
