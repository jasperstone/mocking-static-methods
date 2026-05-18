using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests.Auditing;

public class AbpAuditHubFilterTests
{
    [Fact]
    public async Task ShouldWriteAuditLogAsync_WithEnabledAuditing_ReturnsTrue()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAuditingManager>(Mock.Of<IAuditingManager>(m => m.Current == new AuditLogInfo()))
            .AddSingleton<ICurrentUser>(Mock.Of<ICurrentUser>(m => m.IsAuthenticated == true))
            .AddOptions<AbpAuditingOptions>()
            .Services
            .BuildServiceProvider();

        var filter = new AbpAuditHubFilter();
        var auditLogInfo = new AuditLogInfo();
        var hasError = false;

        // Act
        var result = await GetShouldWriteAuditLogAsyncResult(filter, auditLogInfo, serviceProvider, hasError);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_WithDisabledAuditing_ReturnsFalse()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAuditingManager>(Mock.Of<IAuditingManager>(m => m.Current == null))
            .AddSingleton<ICurrentUser>(Mock.Of<ICurrentUser>(m => m.IsAuthenticated == false))
            .AddOptions<AbpAuditingOptions>()
            .Services
            .BuildServiceProvider();

        var filter = new AbpAuditHubFilter();
        var auditLogInfo = new AuditLogInfo();
        var hasError = false;

        // Act
        var result = await GetShouldWriteAuditLogAsyncResult(filter, auditLogInfo, serviceProvider, hasError);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_WithAlwaysLogOnException_ReturnsTrue()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAuditingManager>(Mock.Of<IAuditingManager>(m => m.Current == new AuditLogInfo()))
            .AddSingleton<ICurrentUser>(Mock.Of<ICurrentUser>(m => m.IsAuthenticated == true))
            .AddOptions<AbpAuditingOptions>(options =>
            {
                options.AlwaysLogOnException = true;
            })
            .Services
            .BuildServiceProvider();

        var filter = new AbpAuditHubFilter();
        var auditLogInfo = new AuditLogInfo();
        var hasError = true;

        // Act
        var result = await GetShouldWriteAuditLogAsyncResult(filter, auditLogInfo, serviceProvider, hasError);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShouldWriteAuditLogAsync_WithAlwaysLogSelectors_ReturnsTrue()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAuditingManager>(Mock.Of<IAuditingManager>(m => m.Current == new AuditLogInfo()))
            .AddSingleton<ICurrentUser>(Mock.Of<ICurrentUser>(m => m.IsAuthenticated == true))
            .AddOptions<AbpAuditingOptions>(options =>
            {
                options.AlwaysLogSelectors.Add(async (auditLogInfo) =>
                {
                    return await Task.FromResult(true);
                });
            })
            .Services
            .BuildServiceProvider();

        var filter = new AbpAuditHubFilter();
        var auditLogInfo = new AuditLogInfo();
        var hasError = false;

        // Act
        var result = await GetShouldWriteAuditLogAsyncResult(filter, auditLogInfo, serviceProvider, hasError);

        // Assert
        Assert.True(result);
    }

    private async Task<bool> GetShouldWriteAuditLogAsyncResult(AbpAuditHubFilter filter, AuditLogInfo auditLogInfo, IServiceProvider serviceProvider, bool hasError)
    {
        return await (Task<bool>)typeof(AbpAuditHubFilter)
            .GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(filter, new object[] { auditLogInfo, serviceProvider, hasError });
    }
}
