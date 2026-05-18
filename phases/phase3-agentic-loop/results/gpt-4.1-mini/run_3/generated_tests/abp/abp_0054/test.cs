using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Auditing;
using Volo.Abp.Users;
using Volo.Abp.Uow;
using Volo.Abp.AspNetCore.SignalR.Auditing;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Auditing.Tests
{
    public class AbpAuditHubFilterTests
    {
        private class FakeAuditLogScope : IAuditLogScope
        {
            public AuditLogInfo Log { get; }

            public FakeAuditLogScope(AuditLogInfo log)
            {
                Log = log;
            }
        }

        private class FakeAuditLogSaveHandle : IAuditLogSaveHandle
        {
            public bool SaveCalled { get; private set; }

            public Task SaveAsync()
            {
                SaveCalled = true;
                return Task.CompletedTask;
            }

            public void Dispose()
            {
            }
        }

        private class FakeAuditingManager : IAuditingManager
        {
            public IAuditLogScope? Current { get; set; }
            public FakeAuditLogSaveHandle SaveHandle { get; } = new();

            public IAuditLogSaveHandle BeginScope()
            {
                return SaveHandle;
            }
        }

        private class FakeUnitOfWork : IUnitOfWork
        {
            public Guid Id { get; } = Guid.NewGuid();
            public Dictionary<string, object> Items { get; } = new();
            public event EventHandler<UnitOfWorkFailedEventArgs>? Failed;
            public event EventHandler<UnitOfWorkEventArgs>? Disposed;
            public IAbpUnitOfWorkOptions Options { get; } = new AbpUnitOfWorkOptions();
            public IUnitOfWork? Outer { get; private set; }
            public bool IsReserved { get; private set; }
            public bool IsDisposed { get; private set; }
            public bool IsCompleted { get; private set; }
            public string? ReservationName { get; private set; }

            public void SetOuter(IUnitOfWork? outer)
            {
                Outer = outer;
            }

            public void Initialize(AbpUnitOfWorkOptions options)
            {
            }

            public void Reserve(string reservationName)
            {
                ReservationName = reservationName;
                IsReserved = true;
            }

            public Task SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task CompleteAsync(System.Threading.CancellationToken cancellationToken = default)
            {
                IsCompleted = true;
                return Task.CompletedTask;
            }

            public Task RollbackAsync(System.Threading.CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public void OnCompleted(Func<Task> handler)
            {
            }

            public void AddOrReplaceLocalEvent(UnitOfWorkEventRecord eventRecord, Predicate<UnitOfWorkEventRecord>? replacementSelector = null)
            {
            }

            public void AddOrReplaceDistributedEvent(UnitOfWorkEventRecord eventRecord, Predicate<UnitOfWorkEventRecord>? replacementSelector = null)
            {
            }

            public void Dispose()
            {
                IsDisposed = true;
                Disposed?.Invoke(this, new UnitOfWorkEventArgs(this));
            }
        }

        private class FakeUnitOfWorkManager : IUnitOfWorkManager
        {
            public IUnitOfWork? Current { get; set; }
        }

        private class FakeCurrentUser : ICurrentUser
        {
            public bool IsAuthenticated { get; set; }
        }

        private class FakeServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _services = new();

            public void AddService<T>(T service) => _services[typeof(T)] = service!;

            public object? GetService(Type serviceType) => _services.TryGetValue(serviceType, out var service) ? service : null;
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogSelectorReturnsTrue()
        {
            var filter = new AbpAuditHubFilter();

            var auditLogInfo = new AuditLogInfo();

            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions();
            options.AlwaysLogSelectors.Add(_ => Task.FromResult(true));
            optionsMock.Setup(o => o.Value).Returns(options);

            var currentUser = new FakeCurrentUser { IsAuthenticated = true };
            var auditingManager = new FakeAuditingManager
            {
                Current = new FakeAuditLogScope(new AuditLogInfo())
            };

            var serviceProvider = new FakeServiceProvider();
            serviceProvider.AddService(optionsMock.Object);
            serviceProvider.AddService<ICurrentUser>(currentUser);
            serviceProvider.AddService<IAuditingManager>(auditingManager);

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<bool>)method.Invoke(filter, new object[] { auditLogInfo, serviceProvider, false })!;
            var result = await task;

            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsTrue_WhenAlwaysLogOnExceptionAndHasError()
        {
            var filter = new AbpAuditHubFilter();

            var auditLogInfo = new AuditLogInfo();

            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions
            {
                AlwaysLogOnException = true
            };
            optionsMock.Setup(o => o.Value).Returns(options);

            var currentUser = new FakeCurrentUser { IsAuthenticated = true };
            var auditingManager = new FakeAuditingManager
            {
                Current = new FakeAuditLogScope(new AuditLogInfo())
            };

            var serviceProvider = new FakeServiceProvider();
            serviceProvider.AddService(optionsMock.Object);
            serviceProvider.AddService<ICurrentUser>(currentUser);
            serviceProvider.AddService<IAuditingManager>(auditingManager);

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<bool>)method.Invoke(filter, new object[] { auditLogInfo, serviceProvider, true })!;
            var result = await task;

            Assert.True(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAnonymousUserNotEnabled()
        {
            var filter = new AbpAuditHubFilter();

            var auditLogInfo = new AuditLogInfo();

            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions
            {
                IsEnabledForAnonymousUsers = false
            };
            optionsMock.Setup(o => o.Value).Returns(options);

            var currentUser = new FakeCurrentUser { IsAuthenticated = false };
            var auditingManager = new FakeAuditingManager
            {
                Current = new FakeAuditLogScope(new AuditLogInfo())
            };

            var serviceProvider = new FakeServiceProvider();
            serviceProvider.AddService(optionsMock.Object);
            serviceProvider.AddService<ICurrentUser>(currentUser);
            serviceProvider.AddService<IAuditingManager>(auditingManager);

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<bool>)method.Invoke(filter, new object[] { auditLogInfo, serviceProvider, false })!;
            var result = await task;

            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenAuditingManagerCurrentIsNull()
        {
            var filter = new AbpAuditHubFilter();

            var auditLogInfo = new AuditLogInfo();

            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions();
            optionsMock.Setup(o => o.Value).Returns(options);

            var currentUser = new FakeCurrentUser { IsAuthenticated = true };
            var auditingManager = new FakeAuditingManager
            {
                Current = null
            };

            var serviceProvider = new FakeServiceProvider();
            serviceProvider.AddService(optionsMock.Object);
            serviceProvider.AddService<ICurrentUser>(currentUser);
            serviceProvider.AddService<IAuditingManager>(auditingManager);

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<bool>)method.Invoke(filter, new object[] { auditLogInfo, serviceProvider, false })!;
            var result = await task;

            Assert.False(result);
        }

        [Fact]
        public async Task ShouldWriteAuditLogAsync_ReturnsFalse_WhenActionsIsNullOrEmpty()
        {
            var filter = new AbpAuditHubFilter();

            var auditLogInfo = new AuditLogInfo();

            var optionsMock = new Mock<IOptions<AbpAuditingOptions>>();
            var options = new AbpAuditingOptions();
            optionsMock.Setup(o => o.Value).Returns(options);

            var currentUser = new FakeCurrentUser { IsAuthenticated = true };
            var auditLog = new AuditLogInfo();
            auditLog.Actions = null; // Null or empty list
            var auditingManager = new FakeAuditingManager
            {
                Current = new FakeAuditLogScope(auditLog)
            };

            var serviceProvider = new FakeServiceProvider();
            serviceProvider.AddService(optionsMock.Object);
            serviceProvider.AddService<ICurrentUser>(currentUser);
            serviceProvider.AddService<IAuditingManager>(auditingManager);

            var method = typeof(AbpAuditHubFilter).GetMethod("ShouldWriteAuditLogAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<bool>)method.Invoke(filter, new object[] { auditLogInfo, serviceProvider, false })!;
            var result = await task;

            Assert.False(result);
        }
    }
}
