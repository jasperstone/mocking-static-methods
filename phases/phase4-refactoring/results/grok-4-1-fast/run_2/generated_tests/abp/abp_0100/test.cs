using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Permissions.Tests;

public class TestState : IHasSimpleStateCheckers<TestState>
{
    public List<ISimpleStateChecker<TestState>> StateCheckers { get; } = new();
    
    public void ConfigureSimpleStateCheckers(ISimpleStateCheckerBuilder<TestState> builder)
    {
    }
}

public class RequirePermissionsSimpleStateCheckerTests
{
    [Fact]
    public async Task Should_Call_GetRequiredService_On_ServiceProvider()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object);

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1" }
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IPermissionChecker>(), Times.Once);
    }

    [Fact]
    public async Task Should_Return_True_When_Single_Permission_Granted()
    {
        // Arrange
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        permissionCheckerMock.Setup(x => x.IsGrantedAsync("Permission1")).ReturnsAsync(true);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(permissionCheckerMock.Object)
            .BuildServiceProvider();

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProvider);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1" }
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_When_Single_Permission_NotGranted()
    {
        // Arrange
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        permissionCheckerMock.Setup(x => x.IsGrantedAsync("Permission1")).ReturnsAsync(false);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(permissionCheckerMock.Object)
            .BuildServiceProvider();

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProvider);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1" }
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_True_When_All_Multiple_Permissions_Granted_And_RequiresAll()
    {
        // Arrange
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(
            new MultiplePermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                ["Permission1"] = PermissionGrantResult.Granted,
                ["Permission2"] = PermissionGrantResult.Granted
            }));

        var serviceProvider = new ServiceCollection()
            .AddSingleton(permissionCheckerMock.Object)
            .BuildServiceProvider();

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProvider);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1", "Permission2" }
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_When_NotAll_Multiple_Permissions_Granted_And_RequiresAll()
    {
        // Arrange
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(
            new MultiplePermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                ["Permission1"] = PermissionGrantResult.Granted,
                ["Permission2"] = PermissionGrantResult.Undefined
            }));

        var serviceProvider = new ServiceCollection()
            .AddSingleton(permissionCheckerMock.Object)
            .BuildServiceProvider();

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProvider);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1", "Permission2" }
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_True_When_Any_Multiple_Permission_Granted_And_NotRequiresAll()
    {
        // Arrange
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        permissionCheckerMock.Setup(x => x.IsGrantedAsync(It.IsAny<string[]>())).ReturnsAsync(
            new MultiplePermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                ["Permission1"] = PermissionGrantResult.Granted,
                ["Permission2"] = PermissionGrantResult.Undefined
            }));

        var serviceProvider = new ServiceCollection()
            .AddSingleton(permissionCheckerMock.Object)
            .BuildServiceProvider();

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProvider);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1", "Permission2" },
            requiresAll: false
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.True(result);
    }
}
