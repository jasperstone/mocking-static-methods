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
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IPermissionChecker>())
            .Returns(permissionCheckerMock.Object)
            .Verifiable();

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(new MockState(), new[] { "Permission1" });
        var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

        var contextMock = new Mock<SimpleStateCheckerContext<MockState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new MockPermissionGrantResult(new Dictionary<string, MockPermissionGrantResult>
            {
                ["Permission1"] = MockPermissionGrantResult.Granted
            }));

        // Act
        await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IPermissionChecker>(), Times.Once);
    }

    [Fact]
    public async Task Should_Return_True_For_Single_Granted_Permission()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(new MockState(), new[] { "Permission1" });
        var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

        var contextMock = new Mock<SimpleStateCheckerContext<MockState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync("Permission1"))
            .ReturnsAsync(true);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_For_Single_Denied_Permission()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(new MockState(), new[] { "Permission1" });
        var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

        var contextMock = new Mock<SimpleStateCheckerContext<MockState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync("Permission1"))
            .ReturnsAsync(false);

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_True_For_Multiple_Permissions_When_RequiresAll_True_And_All_Granted()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(new MockState(), new[] { "Permission1", "Permission2" });
        var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

        var contextMock = new Mock<SimpleStateCheckerContext<MockState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new MockPermissionGrantResult(new Dictionary<string, MockPermissionGrantResult>
            {
                ["Permission1"] = MockPermissionGrantResult.Granted,
                ["Permission2"] = MockPermissionGrantResult.Granted
            }));

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_For_Multiple_Permissions_When_RequiresAll_True_And_Not_All_Granted()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(new MockState(), new[] { "Permission1", "Permission2" });
        var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

        var contextMock = new Mock<SimpleStateCheckerContext<MockState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new MockPermissionGrantResult(new Dictionary<string, MockPermissionGrantResult>
            {
                ["Permission1"] = MockPermissionGrantResult.Granted,
                ["Permission2"] = MockPermissionGrantResult.Denied
            }));

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_True_For_Multiple_Permissions_When_RequiresAll_False_And_Any_Granted()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(new MockState(), new[] { "Permission1", "Permission2" }, requiresAll: false);
        var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

        var contextMock = new Mock<SimpleStateCheckerContext<MockState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new MockPermissionGrantResult(new Dictionary<string, MockPermissionGrantResult>
            {
                ["Permission1"] = MockPermissionGrantResult.Granted,
                ["Permission2"] = MockPermissionGrantResult.Denied
            }));

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Return_False_For_Multiple_Permissions_When_RequiresAll_False_And_None_Granted()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var permissionCheckerMock = new Mock<IPermissionChecker>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IPermissionChecker>()).Returns(permissionCheckerMock.Object);

        var model = new RequirePermissionsSimpleBatchStateCheckerModel<MockState>(new MockState(), new[] { "Permission1", "Permission2" }, requiresAll: false);
        var checker = new RequirePermissionsSimpleStateChecker<MockState>(model);

        var contextMock = new Mock<SimpleStateCheckerContext<MockState>>();
        contextMock.Setup(c => c.ServiceProvider).Returns(serviceProviderMock.Object);

        permissionCheckerMock
            .Setup(pc => pc.IsGrantedAsync(It.IsAny<string[]>()))
            .ReturnsAsync(new MockPermissionGrantResult(new Dictionary<string, MockPermissionGrantResult>
            {
                ["Permission1"] = MockPermissionGrantResult.Denied,
                ["Permission2"] = MockPermissionGrantResult.Denied
            }));

        // Act
        var result = await checker.IsEnabledAsync(contextMock.Object);

        // Assert
        Assert.False(result);
    }
}

// Mock types for testing
public class MockState : IHasSimpleStateCheckers<MockState>
{
    public List<ISimpleStateChecker<MockState>> StateCheckers { get; } = new();

    MockState IHasSimpleStateCheckers<MockState>.GetSelf() => this;
}

public interface IPermissionChecker
{
    Task<bool> IsGrantedAsync(string name);
    Task<PermissionGrantResult> IsGrantedAsync(params string[] names);
}

public class MockPermissionGrantResult
{
    public Dictionary<string, MockPermissionGrantResult> Result { get; }

    public bool AllGranted => Result != null && Result.Values.All(v => v == Granted);

    public MockPermissionGrantResult(Dictionary<string, MockPermissionGrantResult> result)
    {
        Result = result;
    }

    public static readonly MockPermissionGrantResult Granted = new(null!);
    public static readonly MockPermissionGrantResult Denied = new(null!);
}
