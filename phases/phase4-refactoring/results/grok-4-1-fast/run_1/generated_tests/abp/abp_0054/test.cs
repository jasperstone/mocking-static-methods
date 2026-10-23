using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing;

public class AbpAuditHubFilterTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IOptions<AbpAuditingOptions>> _optionsMock;
    private readonly Mock<IAuditingManager> _auditingManagerMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly AbpAuditHubFilter _filter;

    public AbpAuditHubFilterTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
        _auditingManagerMock = new Mock<IAuditingManager>();
        _currentUserMock = new Mock<ICurrentUser>();

        SetupServiceProvider();

        _filter = new AbpAuditHubFilter();
    }

    private void SetupServiceProvider()
    {
        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpAuditingOptions>>())
            .Returns(_optionsMock.Object);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IAuditingManager>())
            .Returns(_auditingManagerMock.Object);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ICurrentUser>())
            .Returns(_currentUserMock.Object);
    }

    [Fact]
    public async Task InvokeMethodAsync_ShouldCallGetRequiredServiceIAuditingManager()
    {
        // Arrange
        var auditingOptions = new AbpAuditingOptions { IsEnabled = true };
        _optionsMock.Setup(o => o.Value).Returns(auditingOptions);

        var nextMock = new Mock<Func<HubInvocationContext, ValueTask<object?>>>();
        var invocationContextMock = CreateInvocationContextMock();

        _auditingManagerMock.Setup(m => m.BeginScope()).Returns(new Mock<IAuditLogSaveHandle>().Object);
        _auditingManagerMock.Setup(m => m.Current).Returns(new Mock<IAuditLogScope>().Object);

        nextMock.Setup(f => f(It.IsAny<HubInvocationContext>())).ReturnsAsync((object?)null);

        // Act
        await _filter.InvokeMethodAsync(invocationContextMock.Object, nextMock.Object);

        // Assert - Verifies GetRequiredService<IAuditingManager>() call coverage
        _serviceProviderMock.Verify(sp => sp.GetRequiredService<IAuditingManager>(), Times.Once);
    }

    [Fact]
    public async Task InvokeMethodAsync_SkipsAuditing_WhenDisabled()
    {
        // Arrange
        var auditingOptions = new AbpAuditingOptions { IsEnabled = false };
        _optionsMock.Setup(o => o.Value).Returns(auditingOptions);

        var nextMock = new Mock<Func<HubInvocationContext, ValueTask<object?>>>();
        var invocationContextMock = CreateInvocationContextMock();

        nextMock.Setup(f => f(It.IsAny<HubInvocationContext>())).ReturnsAsync("result");

        // Act
        var result = await _filter.InvokeMethodAsync(invocationContextMock.Object, nextMock.Object);

        // Assert - No GetRequiredService<IAuditingManager>() call when auditing disabled
        _serviceProviderMock.Verify(sp => sp.GetRequiredService<IAuditingManager>(), Times.Never);
        Assert.Equal("result", result);
    }

    private Mock<HubInvocationContext> CreateInvocationContextMock()
    {
        var invocationContextMock = new Mock<HubInvocationContext>(
            MockBehavior.Strict,
            new object[] { _serviceProviderMock.Object, Array.Empty<object>(), "TestMethod", Array.Empty<object>() });
        
        invocationContextMock.Setup(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);
        return invocationContextMock;
    }
}
