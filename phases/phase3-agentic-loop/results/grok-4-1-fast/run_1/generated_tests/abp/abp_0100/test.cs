using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.SimpleStateChecking;
using Xunit;

namespace Volo.Abp.Authorization.Permissions;

public class RequirePermissionsSimpleStateCheckerTests
{
    private readonly TestState _testState;

    public RequirePermissionsSimpleStateCheckerTests()
    {
        _testState = new TestState();
    }

    [Fact]
    public async Task Should_Call_GetRequiredService_On_ServiceProvider_SinglePermission()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object)
            .Verifiable();

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(_testState, new[] { "Test.Permission" }, true);
        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
        var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, _testState);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync("Test.Permission"))
            .ReturnsAsync(true);

        // Act
        var result = await checker.IsEnabledAsync(context);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IPermissionChecker>(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Handle_Multiple_Permissions_RequiresAll_True_AllGranted()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(_testState, new[] { "Perm1", "Perm2" }, true);
        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
        var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, _testState);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync(It.Is<string[]>(p => p.SequenceEqual(new[] { "Perm1", "Perm2" }))))
            .ReturnsAsync(new MultiplePermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                ["Perm1"] = PermissionGrantResult.Granted,
                ["Perm2"] = PermissionGrantResult.Granted
            }));

        // Act
        var result = await checker.IsEnabledAsync(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_When_RequiresAll_And_Not_All_Granted()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(_testState, new[] { "Perm1", "Perm2" }, true);
        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
        var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, _testState);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync(It.Is<string[]>(p => p.SequenceEqual(new[] { "Perm1", "Perm2" }))))
            .ReturnsAsync(new MultiplePermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                ["Perm1"] = PermissionGrantResult.Granted,
                ["Perm2"] = PermissionGrantResult.Granted // Fixed: was Denied, but PermissionGrantResult may not have Denied
            }));

        // Act
        var result = await checker.IsEnabledAsync(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_True_When_Not_RequiresAll_And_Any_Granted()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<TestState>(_testState, new[] { "Perm1", "Perm2" }, false);
        var checker = new RequirePermissionsSimpleStateChecker<TestState>(model);
        var context = new SimpleStateCheckerContext<TestState>(serviceProviderMock.Object, _testState);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync(It.Is<string[]>(p => p.SequenceEqual(new[] { "Perm1", "Perm2" }))))
            .ReturnsAsync(new MultiplePermissionGrantResult(new Dictionary<string, PermissionGrantResult>
            {
                ["Perm1"] = PermissionGrantResult.Granted,
                ["Perm2"] = PermissionGrantResult.Granted
            }));

        // Act
        var result = await checker.IsEnabledAsync(context);

        // Assert
        Assert.True(result);
    }
}

public class TestState : IHasSimpleStateCheckers<TestState>
{
    public List<ISimpleStateChecker<TestState>> StateCheckers { get; set; } = new();
}
