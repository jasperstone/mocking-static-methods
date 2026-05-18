using System;
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
        private readonly Mock<AbpAsyncTimer> _timerMock;
        private readonly BackgroundJobWorker _worker;
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
            _lockHandleMock = new Mock<IDistributedLockHandle>();
            _timerMock = new Mock<AbpAsyncTimer>();

            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);

            var jobOptions = Options.Create(new AbpBackgroundJobOptions());
            var workerOptions = Options.Create(new AbpBackgroundJobWorkerOptions());

            _worker = new BackgroundJobWorker(
                _timerMock.Object,
                jobOptions,
                workerOptions,
                _serviceScopeFactoryMock.Object,
                _distributedLockMock.Object);

            // Setup the timer period
            _worker.Timer.Period = TimeSpan.FromSeconds(10);
        }

        [Fact]
        public async Task DoWorkAsync_Should_AcquireLock_And_ProcessJobs()
        {
            // Arrange
            var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var jobInfo = new BackgroundJobInfo
            {
                Id = Guid.NewGuid().ToString(),
                JobName = "TestJob",
                JobArgs = new byte[0],
                TryCount = 0,
                LastTryTime = DateTime.UtcNow.AddMinutes(-5),
                CreationTime = DateTime.UtcNow.AddMinutes(-10)
            };
            var waitingJobs = new[] { jobInfo };

            // Setup lock acquisition
            _distributedLockMock.Setup(d => d.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_lockHandleMock.Object);
            _lockHandleMock.Setup(l => l.DisposeAsync()).Returns(ValueTask.CompletedTask);

            // Setup service provider to return required services
            _serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IBackgroundJobStore)))
                .Returns(_jobStoreMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IBackgroundJobExecuter)))
                .Returns(_jobExecuterMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IClock)))
                .Returns(_clockMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IBackgroundJobSerializer)))
                .Returns(_serializerMock.Object);

            // Setup store to return waiting jobs
            _jobStoreMock.Setup(s => s.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(waitingJobs);

            // Setup JobOptions
            var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var jobOptionsValue = new AbpBackgroundJobOptions();
            var jobMock = new Mock<IBackgroundJob>();
            jobMock.Setup(j => j.ArgsType).Returns(typeof(string));
            var jobOptionsInstance = new AbpBackgroundJobOptions();
            var jobOptions = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var jobOptionsVal = new AbpBackgroundJobOptions();
            var jobOptionsObj = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var jobOptionsValObj = new AbpBackgroundJobOptions();
            var jobOptionsObj2 = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var jobOptionsVal2 = new AbpBackgroundJobOptions();
            var jobOptionsInstance2 = new AbpBackgroundJobOptions();
            var jobOptionsInstance3 = new AbpBackgroundJobOptions();

            // For simplicity, set JobOptions property directly
            _worker.JobOptions = new AbpBackgroundJobOptions();
            _worker.JobOptions.GetType().GetProperty("Jobs").SetValue(_worker.JobOptions, new[] { new BackgroundJobConfiguration { JobType = typeof(string), ArgsType = typeof(string), JobName = "TestJob" } });
            // Mock GetJob to return the job configuration
            var jobConfiguration = new BackgroundJobConfiguration
            {
                JobType = typeof(string),
                ArgsType = typeof(string),
                JobName = "TestJob"
            };
            var jobOptionsInstanceMock = new Mock<AbpBackgroundJobOptions>();
            jobOptionsInstanceMock.Setup(j => j.GetJob(It.IsAny<string>())).Returns(jobConfiguration);
            // Assign the mock to JobOptions
            _worker.GetType().GetProperty("JobOptions").SetValue(_worker, jobOptionsInstanceMock.Object);

            // Setup serializer to deserialize
            _serializerMock.Setup(s => s.Deserialize(It.IsAny<byte[]>(), typeof(string)))
                .Returns("DeserializedArgs");

            // Setup execute async
            _jobExecuterMock.Setup(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _worker.DoWorkAsync(workerContextMock.Object);

            // Assert
            _distributedLockMock.Verify(d => d.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _jobStoreMock.Verify(s => s.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _jobExecuterMock.Verify(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Once);
            _jobStoreMock.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
