using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_ShouldCallGetRequiredServiceForDependencies()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            var clockMock = new Mock<IClock>();
            var backgroundJobSerializerMock = new Mock<IBackgroundJobSerializer>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
                .Returns(backgroundJobStoreMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>())
                .Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IClock>())
                .Returns(clockMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>())
                .Returns(backgroundJobSerializerMock.Object);

            var jobOptions = new AbpBackgroundJobOptions();
            var workerOptions = new AbpBackgroundJobWorkerOptions
            {
                JobPollPeriod = TimeSpan.FromSeconds(10),
                DistributedLockName = "TestLock",
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10
            };

            var timer = new AbpAsyncTimer();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeFactoryMock.Setup(sf => sf.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(ss => ss.ServiceProvider).Returns(serviceProviderMock.Object);

            var workerContext = new PeriodicBackgroundWorkerContext(
                serviceProviderMock.Object,
                CancellationToken.None);

            var worker = new TestableBackgroundJobWorker(
                timer,
                Options.Create(jobOptions),
                Options.Create(workerOptions),
                serviceScopeFactoryMock.Object,
                distributedLockMock.Object);

            // Act
            await worker.DoWorkAsync(workerContext);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobExecuter>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IClock>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobSerializer>(), Times.Once);
        }
    }

    public class TestableBackgroundJobWorker : BackgroundJobWorker
    {
        public TestableBackgroundJobWorker(
            AbpAsyncTimer timer,
            IOptions<AbpBackgroundJobOptions> jobOptions,
            IOptions<AbpBackgroundJobWorkerOptions> workerOptions,
            IServiceScopeFactory serviceScopeFactory,
            IAbpDistributedLock distributedLock)
            : base(timer, jobOptions, workerOptions, serviceScopeFactory, distributedLock)
        {
        }

        public new Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            return base.DoWorkAsync(workerContext);
        }
    }
}
