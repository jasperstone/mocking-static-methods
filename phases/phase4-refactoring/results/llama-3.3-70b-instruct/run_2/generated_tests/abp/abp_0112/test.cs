using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IAbpDistributedLock> _distributedLockMock;
        private readonly Mock<IOptions<AbpBackgroundJobOptions>> _jobOptionsMock;
        private readonly Mock<IOptions<AbpBackgroundJobWorkerOptions>> _workerOptionsMock;
        private readonly Mock<AbpAsyncTimer> _timerMock;

        public BackgroundJobWorkerTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _distributedLockMock = new Mock<IAbpDistributedLock>();
            _jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            _workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            _timerMock = new Mock<AbpAsyncTimer>();
        }

        [Fact]
        public async Task DoWorkAsync_WaitingJobsExist_JobsAreExecuted()
        {
            // Arrange
            var backgroundJobWorker = new BackgroundJobWorker(
                _timerMock.Object,
                _jobOptionsMock.Object,
                _workerOptionsMock.Object,
                _serviceScopeFactoryMock.Object,
                _distributedLockMock.Object);

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBackgroundJobStore>(new Mock<IBackgroundJobStore>().Object)
                .AddSingleton<IBackgroundJobExecuter>(new Mock<IBackgroundJobExecuter>().Object)
                .AddSingleton<IClock>(new Mock<IClock>().Object)
                .AddSingleton<IBackgroundJobSerializer>(new Mock<IBackgroundJobSerializer>().Object)
                .BuildServiceProvider();

            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(new Mock<IServiceScope>().SetupGet(x => x.ServiceProvider).Returns(serviceProvider));

            var waitingJobs = new List<BackgroundJobInfo>
            {
                new BackgroundJobInfo { Id = Guid.NewGuid(), JobName = "TestJob" }
            };

            var backgroundJobStoreMock = serviceProvider.GetService<IBackgroundJobStore>() as Mock<IBackgroundJobStore>;
            backgroundJobStoreMock.Setup(x => x.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(waitingJobs);

            // Act
            await backgroundJobWorker.DoWorkAsync(new PeriodicBackgroundWorkerContext(_serviceScopeFactoryMock.Object, CancellationToken.None));

            // Assert
            backgroundJobStoreMock.Verify(x => x.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task DoWorkAsync_NoWaitingJobs_NoJobsAreExecuted()
        {
            // Arrange
            var backgroundJobWorker = new BackgroundJobWorker(
                _timerMock.Object,
                _jobOptionsMock.Object,
                _workerOptionsMock.Object,
                _serviceScopeFactoryMock.Object,
                _distributedLockMock.Object);

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBackgroundJobStore>(new Mock<IBackgroundJobStore>().Object)
                .AddSingleton<IBackgroundJobExecuter>(new Mock<IBackgroundJobExecuter>().Object)
                .AddSingleton<IClock>(new Mock<IClock>().Object)
                .AddSingleton<IBackgroundJobSerializer>(new Mock<IBackgroundJobSerializer>().Object)
                .BuildServiceProvider();

            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(new Mock<IServiceScope>().SetupGet(x => x.ServiceProvider).Returns(serviceProvider));

            var waitingJobs = new List<BackgroundJobInfo>();

            var backgroundJobStoreMock = serviceProvider.GetService<IBackgroundJobStore>() as Mock<IBackgroundJobStore>;
            backgroundJobStoreMock.Setup(x => x.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(waitingJobs);

            // Act
            await backgroundJobWorker.DoWorkAsync(new PeriodicBackgroundWorkerContext(_serviceScopeFactoryMock.Object, CancellationToken.None));

            // Assert
            backgroundJobStoreMock.Verify(x => x.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
