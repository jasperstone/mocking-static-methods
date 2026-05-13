using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Timing;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IBackgroundJobStore> _jobStoreMock;
        private readonly Mock<IBackgroundJobExecuter> _jobExecuterMock;
        private readonly Mock<IClock> _clockMock;
        private readonly Mock<IBackgroundJobSerializer> _serializerMock;
        private readonly Mock<IAbpDistributedLock> _distributedLockMock;
        private readonly Mock<IDistributedLockProvider> _distributedLockProviderMock;
        private readonly Mock<AbpAsyncTimer> _timerMock;
        private readonly Mock<IOptions<AbpBackgroundJobOptions>> _jobOptionsMock;
        private readonly Mock<IOptions<AbpBackgroundJobWorkerOptions>> _workerOptionsMock;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public BackgroundJobWorkerTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _jobStoreMock = new Mock<IBackgroundJobStore>();
            _jobExecuterMock = new Mock<IBackgroundJobExecuter>();
            _clockMock = new Mock<IClock>();
            _serializerMock = new Mock<IBackgroundJobSerializer>();
            _distributedLockMock = new Mock<IAbpDistributedLock>();
            _timerMock = new Mock<AbpAsyncTimer>();
            _jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            _workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
        }

        [Fact]
        public async Task DoWorkAsync_Should_Call_GetRequiredService_For_IBackgroundJobStore()
        {
            // Arrange
            var workerOptions = new AbpBackgroundJobWorkerOptions
            {
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10,
                JobPollPeriod = TimeSpan.FromSeconds(1),
                DistributedLockName = "TestLock",
                DefaultFirstWaitDuration = 1,
                DefaultWaitFactor = 2,
                DefaultTimeout = 60
            };
            _workerOptionsMock.Setup(o => o.Value).Returns(workerOptions);
            var jobOptions = new AbpBackgroundJobOptions();
            _jobOptionsMock.Setup(o => o.Value).Returns(jobOptions);

            var worker = new BackgroundJobWorker(
                _timerMock.Object,
                _jobOptionsMock.Object,
                _workerOptionsMock.Object,
                _serviceScopeFactoryMock.Object,
                _distributedLockMock.Object);

            var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
            var serviceProvider = new Mock<IServiceProvider>();
            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            var clockMock = new Mock<IClock>();
            var serializerMock = new Mock<IBackgroundJobSerializer>();

            var waitingJobs = new List<BackgroundJobInfo>
            {
                new BackgroundJobInfo { Id = Guid.NewGuid(), JobName = "TestJob", JobArgs = new byte[0], TryCount = 0, CreationTime = DateTime.UtcNow }
            };

            // Setup the service provider to return the mocked dependencies
            serviceProvider.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>()).Returns(backgroundJobStoreMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>()).Returns(backgroundJobExecuterMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IClock>()).Returns(clockMock.Object);
            serviceProvider.Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>()).Returns(serializerMock.Object);

            // Setup workerContext to return the mocked service provider
            workerContextMock.Setup(wc => wc.ServiceProvider).Returns(serviceProvider.Object);
            workerContextMock.Setup(wc => wc.CancellationToken).Returns(_cancellationToken);

            // Setup DistributedLock to simulate successful lock acquisition
            var lockHandleMock = new Mock<IAbpDistributedLockHandle>();
            _distributedLockMock.Setup(dl => dl.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lockHandleMock.Object);

            // Act
            await worker.DoWorkAsync(workerContextMock.Object);

            // Assert
            _serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
        }
    }
}
