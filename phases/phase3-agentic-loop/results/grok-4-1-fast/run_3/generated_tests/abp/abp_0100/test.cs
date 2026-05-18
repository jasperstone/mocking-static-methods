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
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        permissionCheckerMock
            .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new MultiplePermissionGrantResult());
        
        serviceProviderMock
            .Setup(x => x.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object)
            .Verifiable();

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(), 
            new[] { "Permission1" }
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        serviceProviderMock.Verify(x => x.GetRequiredService<IPermissionChecker>(), Times.Once);
    }

    [Fact]
    public async Task Should_Return_True_When_Single_Permission_Granted_And_RequiresAll()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        permissionCheckerMock
            .Setup(x => x.IsGrantedAsync("Permission1"))
            .ReturnsAsync(true);

        serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object);

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1" },
            requiresAll: true
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_When_Single_Permission_NotGranted_And_RequiresAll()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        permissionCheckerMock
            .Setup(x => x.IsGrantedAsync("Permission1"))
            .ReturnsAsync(false);

        serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object);

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1" },
            requiresAll: true
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_True_When_Multiple_Permissions_AllGranted_And_RequiresAll()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        var grantResult = new MultiplePermissionGrantResult
        {
            Result = new Dictionary<string, PermissionGrantResult>
            {
                ["Permission1"] = PermissionGrantResult.Granted,
                ["Permission2"] = PermissionGrantResult.Granted
            }
        };
        permissionCheckerMock
            .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(grantResult);

        serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object);

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1", "Permission2" },
            requiresAll: true
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_When_Multiple_Permissions_NotAllGranted_And_RequiresAll()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        var grantResult = new MultiplePermissionGrantResult
        {
            Result = new Dictionary<string, PermissionGrantResult>
            {
                ["Permission1"] = PermissionGrantResult.Granted,
                ["Permission2"] = PermissionGrantResult.Undefined
            }
        };
        permissionCheckerMock
            .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(grantResult);

        serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object);

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(
            new TestState(),
            new[] { "Permission1", "Permission2" },
            requiresAll: true
        );

        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_True_When_Any_Permission_Granted_And_Not_RequiresAll()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        var grantResult = new MultiplePermissionGrantResult
        {
            Result = new Dictionary<string, PermissionGrantResult>
            {
                ["Permission1"] = PermissionGrantResult.Granted,
                ["Permission2"] = PermissionGrantResult.Undefined
            }
        };
        permissionCheckerMock
            .Setup(x => x.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(grantResult);

        serviceProviderMock.Setup(x => x.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object);

        var contextMock = new Mock<SimpleStateCheckerContext<TestState>>();
        contextMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

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

    private class TestState : IHasSimpleStateCheckers<TestState>
    {
        public SimpleStateCheckerDictionary<TestState> StateCheckers { get; } = new SimpleStateCheckerDictionary<TestState>();
    }
}
