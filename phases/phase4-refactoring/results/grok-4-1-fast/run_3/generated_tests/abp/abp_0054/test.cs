using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            .Setup(sp => sp.GetRequiredService(typeof(IOptions<AbpAuditingOptions>)))
            .Returns(_optionsMock.Object);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(IAuditingManager)))
            .Returns(_auditingManagerMock.Object);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(ICurrentUser)))
            .Returns(_currentUserMock.Object);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_CallsGetRequiredServiceIAuditingManager()
    {
        // Arrange
        var auditLogInfo = new AuditLogInfo();
        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true,
            IsEnabledForAnonymousUsers = true
        };
        _optionsMock.Setup(o => o.Value).Returns(auditingOptions);
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
        _auditingManagerMock.Setup(m => m.Current).Returns((IAuditLogScope?)null);

        // Act
        var result = await InvokePrivateShouldWriteAuditLogAsync(_filter, auditLogInfo, _serviceProviderMock.Object, false);

        // Assert - Verifies the GetRequiredService<IAuditingManager> call on line 101
        _serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IAuditingManager)), Times.Once);
        _serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IOptions<AbpAuditingOptions>)), Times.AtLeastOnce);
        _serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(ICurrentUser)), Times.Once);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_CurrentScopeNull_ReturnsFalse()
    {
        // Arrange
        var auditLogInfo = new AuditLogInfo();
        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true,
            IsEnabledForAnonymousUsers = true
        };
        _optionsMock.Setup(o => o.Value).Returns(auditingOptions);
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
        _auditingManagerMock.Setup(m => m.Current).Returns((IAuditLogScope?)null);

        // Act
        var result = await InvokePrivateShouldWriteAuditLogAsync(_filter, auditLogInfo, _serviceProviderMock.Object, false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_EmptyActions_ReturnsFalse()
    {
        // Arrange
        var auditLogInfo = new AuditLogInfo { Actions = new List<AuditLogActionInfo>() };
        var scopeMock = new Mock<IAuditLogScope>();
        scopeMock.Setup(s => s.Log).Returns(auditLogInfo);

        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true,
            IsEnabledForAnonymousUsers = true
        };
        _optionsMock.Setup(o => o.Value).Returns(auditingOptions);
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
        _auditingManagerMock.Setup(m => m.Current).Returns(scopeMock.Object);

        // Act
        var result = await InvokePrivateShouldWriteAuditLogAsync(_filter, auditLogInfo, _serviceProviderMock.Object, false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_HasActions_ReturnsTrue()
    {
        // Arrange
        var auditLogInfo = new AuditLogInfo 
        { 
            Actions = new List<AuditLogActionInfo> { new AuditLogActionInfo() } 
        };
        var scopeMock = new Mock<IAuditLogScope>();
        scopeMock.Setup(s => s.Log).Returns(auditLogInfo);

        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true,
            IsEnabledForAnonymousUsers = true
        };
        _optionsMock.Setup(o => o.Value).Returns(auditingOptions);
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
        _auditingManagerMock.Setup(m => m.Current).Returns(scopeMock.Object);

        // Act
        var result = await InvokePrivateShouldWriteAuditLogAsync(_filter, auditLogInfo, _serviceProviderMock.Object, false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_AnonymousNotAllowedAndNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var auditLogInfo = new AuditLogInfo();
        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true,
            IsEnabledForAnonymousUsers = false
        };
        _optionsMock.Setup(o => o.Value).Returns(auditingOptions);
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(false);

        // Act
        var result = await InvokePrivateShouldWriteAuditLogAsync(_filter, auditLogInfo, _serviceProviderMock.Object, false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_ExceptionAndAlwaysLogOnException_ReturnsTrue()
    {
        // Arrange
        var auditLogInfo = new AuditLogInfo();
        var auditingOptions = new AbpAuditingOptions
        {
            IsEnabled = true,
            IsEnabledForAnonymousUsers = true,
            AlwaysLogOnException = true
        };
        _optionsMock.Setup(o => o.Value).Returns(auditingOptions);
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);

        // Act
        var result = await InvokePrivateShouldWriteAuditLogAsync(_filter, auditLogInfo, _serviceProviderMock.Object, true);

        // Assert
        Assert.True(result);
    }

    private static async Task<bool> InvokePrivateShouldWriteAuditLogAsync(AbpAuditHubFilter filter, AuditLogInfo auditLogInfo, IServiceProvider serviceProvider, bool hasError)
    {
        var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return await (Task<bool>)method.Invoke(filter, new object[] { auditLogInfo, serviceProvider, hasError })!;
    }
}
