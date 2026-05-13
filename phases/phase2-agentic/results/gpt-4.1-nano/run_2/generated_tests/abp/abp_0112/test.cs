using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Timing;
using Volo.Abp.BackgroundWorkers;

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
        private readonly Mock<AbpAsyncTimer> _timerMock;
        private readonly Mock<ILogger<BackgroundJobWorker>> _loggerMock;

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
            _timerMock = new Mock<AbpAsyncTimer>();
            _loggerMock = new Mock<ILogger<BackgroundJobWorker>>();

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
        }

        [Fact]
        public async Task DoWorkAsync_Should_AcquireLock_And_ProcessJobs()
        {
            // Arrange
            var workerOptions = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            var jobOptions = new Mock<IOptions<AbpBackgroundJobOptions>>();

            var workerOpts = new AbpBackgroundJobWorkerOptions
            {
                JobPollPeriod = TimeSpan.FromSeconds(1),
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10,
                DefaultFirstWaitDuration = 1,
                DefaultWaitFactor = 2,
                DefaultTimeout = 60
            };
            var jobOpts = new AbpBackgroundJobOptions();
            var jobConfig = new
            {
                JobType = typeof(object),
                ArgsType = typeof(object),
                JobName = "TestJob"
            };

            workerOptions.Setup(w => w.Value).Returns(workerOpts);
            jobOptions.Setup(j => j.Value).Returns(jobOpts);

            var workerContext = new Mock<PeriodicBackgroundWorkerContext>();
            var serviceProvider = new Mock<IServiceProvider>();
            var jobInfo = new BackgroundJobInfo
            {
                Id = Guid.NewGuid().ToString(),
                JobName = "TestJob",
                JobArgs = "args",
                TryCount = 0,
                LastTryTime = DateTime.Now.AddMinutes(-5),
                CreationTime = DateTime.Now.AddMinutes(-10),
                IsAbandoned = false
            };
            var waitingJobs = new[] { jobInfo };

            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
                .Returns(_jobStoreMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>())
                .Returns(_jobExecuterMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IClock>())
                .Returns(_clockMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>())
                .Returns(_serializerMock.Object);

            _clockMock.Setup(c => c.Now).Returns(DateTime.Now);
            _jobStoreMock.Setup(s => s.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(waitingJobs);
            _distributedLockMock.Setup(d => d.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_lockHandleMock.Object);
            _lockHandleMock.Setup(h => h.DisposeAsync()).Returns(ValueTask.CompletedTask);

            var worker = new BackgroundJobWorker(
                _timerMock.Object,
                jobOptions.Object,
                workerOptions.Object,
                _serviceScopeFactoryMock.Object,
                _distributedLockMock.Object);

            workerContext.Setup(w => w.ServiceProvider).Returns(_serviceProviderMock.Object);
            workerContext.Setup(w => w.CancellationToken).Returns(CancellationToken.None);

            // Act
            await worker.DoWorkAsync(workerContext.Object);

            // Assert
            _distributedLockMock.Verify(d => d.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _jobStoreMock.Verify(s => s.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _jobExecuterMock.Verify(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Once);
            _jobStoreMock.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
