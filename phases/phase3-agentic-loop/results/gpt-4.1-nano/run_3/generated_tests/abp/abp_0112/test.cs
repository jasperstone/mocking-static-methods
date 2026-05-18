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
        private readonly Mock<IDistributedLockHandle> _lockHandleMock;
        private readonly Mock<ILogger<BackgroundJobWorker>> _loggerMock;
        private readonly Mock<IOptions<AbpBackgroundJobOptions>> _jobOptionsMock;
        private readonly Mock<IOptions<AbpBackgroundJobWorkerOptions>> _workerOptionsMock;
        private readonly Mock<AbpAsyncTimer> _timerMock;

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
            _lockHandleMock = new Mock<IDistributedLockHandle>();
            _loggerMock = new Mock<ILogger<BackgroundJobWorker>>();
            _jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            _workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            _timerMock = new Mock<AbpAsyncTimer>();

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>()).Returns(_jobStoreMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>()).Returns(_jobExecuterMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IClock>()).Returns(_clockMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>()).Returns(_serializerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>()).Returns(_jobStoreMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>()).Returns(_jobExecuterMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IClock>()).Returns(_clockMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>()).Returns(_serializerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>()).Returns(_jobStoreMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>()).Returns(_jobExecuterMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IClock>()).Returns(_clockMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>()).Returns(_serializerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>()).Returns(_jobStoreMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>()).Returns(_jobExecuterMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IClock>()).Returns(_clockMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>()).Returns(_serializerMock.Object);
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
        }

        [Fact]
        public async Task DoWorkAsync_Should_Call_GetRequiredService_ForBackgroundJobStore()
        {
            // Arrange
            var timer = _timerMock.Object;
            var optionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            var distributedLockMock = _distributedLockMock;
            var worker = new BackgroundJobWorker(
                timer,
                optionsMock.Object,
                workerOptionsMock.Object,
                _serviceScopeFactoryMock.Object,
                distributedLockMock);

            var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
            var serviceProvider = _serviceProviderMock.Object;
            var serviceScope = _serviceScopeMock.Object;

            var jobInfos = new List<BackgroundJobInfo>
            {
                new BackgroundJobInfo { Id = Guid.NewGuid().ToString(), JobName = "TestJob", JobArgs = new byte[0], TryCount = 0, CreationTime = DateTime.UtcNow }
            };

            var waitingJobs = jobInfos.AsEnumerable();

            var lockHandle = _lockHandleMock.Object;

            _distributedLockMock.Setup(dl => dl.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lockHandle);

            _jobStoreMock.Setup(s => s.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(waitingJobs);

            _clockMock.Setup(c => c.Now).Returns(DateTime.UtcNow);

            // Act
            await worker.DoWorkAsync(workerContextMock.Object);

            // Assert
            _jobStoreMock.Verify(s => s.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
