using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Volo.Abp.Users;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests.Auditing;

public class AbpAuditHubFilterTests
{
    [Fact]
    public async Task InvokeMethodAsync_AlwaysLogSelectors_ReturnsTrue()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAuditingManager>(Mock.Of<IAuditingManager>())
            .AddSingleton<ICurrentUser>(Mock.Of<ICurrentUser>())
            .AddSingleton<IOptions<AbpAuditingOptions>>(Mock.Of<IOptions<AbpAuditingOptions>>())
            .BuildServiceProvider();

        var auditLogInfo = new AuditLogInfo();
        var alwaysLogSelectors = new List<Func<AuditLogInfo, Task<bool>>>
        {
            async (info) => true
        };

        var abpAuditingOptions = new AbpAuditingOptions();
        abpAuditingOptions.AlwaysLogSelectors.AddRange(alwaysLogSelectors);

        var options = Mock.Of<IOptions<AbpAuditingOptions>>(o => o.Value == abpAuditingOptions);

        var abpAuditHubFilter = new AbpAuditHubFilter();

        // Act
        var result = await abpAuditHubFilter.InvokeMethodAsync(
            new HubInvocationContext(new HubCallerContext
            {
                ServiceProvider = serviceProvider
            }),
            async (context) => Task.FromResult((object?)null));

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task InvokeMethodAsync_AlwaysLogOnException_ReturnsTrue()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAuditingManager>(Mock.Of<IAuditingManager>())
            .AddSingleton<ICurrentUser>(Mock.Of<ICurrentUser>())
            .AddSingleton<IOptions<AbpAuditingOptions>>(Mock.Of<IOptions<AbpAuditingOptions>>())
            .BuildServiceProvider();

        var auditLogInfo = new AuditLogInfo();
        var abpAuditingOptions = new AbpAuditingOptions
        {
            AlwaysLogOnException = true
        };

        var options = Mock.Of<IOptions<AbpAuditingOptions>>(o => o.Value == abpAuditingOptions);

        var abpAuditHubFilter = new AbpAuditHubFilter();

        // Act
        var result = await abpAuditHubFilter.InvokeMethodAsync(
            new HubInvocationContext(new HubCallerContext
            {
                ServiceProvider = serviceProvider
            }),
            async (context) =>
            {
                try
                {
                    throw new Exception();
                }
                catch (Exception ex)
                {
                    return Task.FromResult((object?)ex);
                }
            });

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task InvokeMethodAsync_IsEnabledForAnonymousUsers_ReturnsTrue()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAuditingManager>(Mock.Of<IAuditingManager>())
            .AddSingleton<ICurrentUser>(Mock.Of<ICurrentUser>(u => u.IsAuthenticated == false))
            .AddSingleton<IOptions<AbpAuditingOptions>>(Mock.Of<IOptions<AbpAuditingOptions>>())
            .BuildServiceProvider();

        var auditLogInfo = new AuditLogInfo();
        var abpAuditingOptions = new AbpAuditingOptions
        {
            IsEnabledForAnonymousUsers = true
        };

        var options = Mock.Of<IOptions<AbpAuditingOptions>>(o => o.Value == abpAuditingOptions);

        var abpAuditHubFilter = new AbpAuditHubFilter();

        // Act
        var result = await abpAuditHubFilter.InvokeMethodAsync(
            new HubInvocationContext(new HubCallerContext
            {
                ServiceProvider = serviceProvider
            }),
            async (context) => Task.FromResult((object?)null));

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task InvokeMethodAsync_AuditingManager_Current_Log_Actions_IsNotEmpty_ReturnsTrue()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAuditingManager>(Mock.Of<IAuditingManager>(m => m.Current == new AuditLogScope(new AuditLogInfo { Actions = new List<AuditLogActionInfo> { new AuditLogActionInfo() } })))
            .AddSingleton<ICurrentUser>(Mock.Of<ICurrentUser>())
            .AddSingleton<IOptions<AbpAuditingOptions>>(Mock.Of<IOptions<AbpAuditingOptions>>())
            .BuildServiceProvider();

        var auditLogInfo = new AuditLogInfo();
        var abpAuditingOptions = new AbpAuditingOptions();

        var options = Mock.Of<IOptions<AbpAuditingOptions>>(o => o.Value == abpAuditingOptions);

        var abpAuditHubFilter = new AbpAuditHubFilter();

        // Act
        var result = await abpAuditHubFilter.InvokeMethodAsync(
            new HubInvocationContext(new HubCallerContext
            {
                ServiceProvider = serviceProvider
            }),
            async (context) => Task.FromResult((object?)null));

        // Assert
        Assert.NotNull(result);
    }
}
