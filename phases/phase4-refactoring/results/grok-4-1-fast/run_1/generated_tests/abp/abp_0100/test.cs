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
    public async Task Should_Call_GetRequiredService_On_ServiceProvider()
    {
        // Arrange
        var mockPermissionChecker = new Mock<IPermissionChecker>();
        mockPermissionChecker
            .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new MultiplePermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                ["Permission1"] = PermissionGrantResult.Granted
            }));

        var services = new ServiceCollection();
        services.AddSingleton(mockPermissionChecker.Object);
        var serviceProvider = services.BuildServiceProvider();

        var mockContext = new Mock<SimpleStateCheckerContext<DummyState>>();
        mockContext.Setup(c => c.ServiceProvider).Returns(serviceProvider);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
            new DummyState(), 
            new[] { "Permission1" });

        var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

        // Act
        var result = await checker.IsEnabledAsync(mockContext.Object);

        // Assert
        mockPermissionChecker.Verify(x => x.IsGrantedAsync(It.Is<string[]>(p => p.SequenceEqual(new[] { "Permission1" }))), Times.Once);
        mockContext.Verify(c => c.ServiceProvider, Times.AtLeastOnce());
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Handle_Single_Permission()
    {
        // Arrange
        var mockPermissionChecker = new Mock<IPermissionChecker>();
        mockPermissionChecker
            .Setup(x => x.IsGrantedAsync("SinglePermission"))
            .ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddSingleton(mockPermissionChecker.Object);
        var serviceProvider = services.BuildServiceProvider();

        var mockContext = new Mock<SimpleStateCheckerContext<DummyState>>();
        mockContext.Setup(c => c.ServiceProvider).Returns(serviceProvider);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
            new DummyState(),
            new[] { "SinglePermission" });

        var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

        // Act
        var result = await checker.IsEnabledAsync(mockContext.Object);

        // Assert
        mockPermissionChecker.Verify(x => x.IsGrantedAsync("SinglePermission"), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_When_RequiresAll_And_Not_All_Granted()
    {
        // Arrange
        var mockPermissionChecker = new Mock<IPermissionChecker>();
        mockPermissionChecker
            .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new MultiplePermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                ["Permission1"] = PermissionGrantResult.Granted,
                ["Permission2"] = PermissionGrantResult.Denied
            }));

        var services = new ServiceCollection();
        services.AddSingleton(mockPermissionChecker.Object);
        var serviceProvider = services.BuildServiceProvider();

        var mockContext = new Mock<SimpleStateCheckerContext<DummyState>>();
        mockContext.Setup(c => c.ServiceProvider).Returns(serviceProvider);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
            new DummyState(),
            new[] { "Permission1", "Permission2" },
            requiresAll: true);

        var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

        // Act
        var result = await checker.IsEnabledAsync(mockContext.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_True_When_Not_RequiresAll_And_Any_Granted()
    {
        // Arrange
        var mockPermissionChecker = new Mock<IPermissionChecker>();
        mockPermissionChecker
            .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new MultiplePermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                ["Permission1"] = PermissionGrantResult.Granted,
                ["Permission2"] = PermissionGrantResult.Denied
            }));

        var services = new ServiceCollection();
        services.AddSingleton(mockPermissionChecker.Object);
        var serviceProvider = services.BuildServiceProvider();

        var mockContext = new Mock<SimpleStateCheckerContext<DummyState>>();
        mockContext.Setup(c => c.ServiceProvider).Returns(serviceProvider);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<DummyState>(
            new DummyState(),
            new[] { "Permission1", "Permission2" },
            requiresAll: false);

        var checker = new RequirePermissionsSimpleStateChecker<DummyState>(model);

        // Act
        var result = await checker.IsEnabledAsync(mockContext.Object);

        // Assert
        Assert.True(result);
    }
}

// Minimal implementations for generic constraints
public class DummyState : IHasSimpleStateCheckers<DummyState>
{
    public List<ISimpleStateChecker<DummyState>> StateCheckers { get; } = new();
}
