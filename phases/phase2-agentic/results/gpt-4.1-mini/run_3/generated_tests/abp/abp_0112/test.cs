using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Timing;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests;

public class BackgroundJobWorkerTests
{
    private class TestBackgroundJobInfo : BackgroundJobInfo
    {
        public TestBackgroundJobInfo()
        {
            Id = Guid.NewGuid();
            JobName = "TestJob";
            JobArgs = "{}";
            CreationTime = DateTime.UtcNow.AddMinutes(-5);
            TryCount = 0;
            IsAbandoned = false;
        }
    }

    [Fact]
    public async Task DoWorkAsync_Should_Call_GetRequiredService_On_ServiceProvider()
    {
        // Arrange
        var jobInfo = new TestBackgroundJobInfo();

        var waitingJobs = new List<BackgroundJobInfo> { jobInfo };

        var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
        var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        var distributedLockMock = new Mock<IAbpDistributedLock>();
        var distributedLockHandlerMock = new Mock<IAsyncDisposable>();

        var jobOptions = new AbpBackgroundJobOptions();
        jobOptions.Jobs["TestJob"] = new BackgroundJobOptions.JobInfo(typeof(object), typeof(object));
        jobOptionsMock.Setup(j => j.Value).Returns(jobOptions);

        var workerOptions = new AbpBackgroundJobWorkerOptions
        {
            ApplicationName = "TestApp",
            MaxJobFetchCount = 10,
            DistributedLockName = "lock",
            JobPollPeriod = 1000,
            DefaultFirstWaitDuration = 1,
            DefaultWaitFactor = 2,
            DefaultTimeout = 1000
        };
        workerOptionsMock.Setup(w => w.Value).Returns(workerOptions);

        distributedLockMock
            .Setup(d => d.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(distributedLockHandlerMock.Object);

        var serviceProviderMock = new Mock<IServiceProvider>();

        var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
        backgroundJobStoreMock
            .Setup(s => s.GetWaitingJobsAsync(workerOptions.ApplicationName, workerOptions.MaxJobFetchCount))
            .ReturnsAsync(waitingJobs);

        var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
        backgroundJobExecuterMock
            .Setup(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()))
            .Returns(Task.CompletedTask);

        var clockMock = new Mock<IClock>();
        clockMock.Setup(c => c.Now).Returns(DateTime.UtcNow);

        var serializerMock = new Mock<IBackgroundJobSerializer>();
        serializerMock
            .Setup(s => s.Deserialize(It.IsAny<string>(), It.IsAny<Type>()))
            .Returns(new object());

        // Setup service provider to return mocks on GetRequiredService calls
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IBackgroundJobStore)))
            .Returns(backgroundJobStoreMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IBackgroundJobExecuter)))
            .Returns(backgroundJobExecuterMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IClock)))
            .Returns(clockMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IBackgroundJobSerializer)))
            .Returns(serializerMock.Object);

        // IServiceScopeFactory and IServiceScope to provide the service provider
        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

        var timer = new AbpAsyncTimer();

        var worker = new BackgroundJobWorker(
            timer,
            jobOptionsMock.Object,
            workerOptionsMock.Object,
            serviceScopeFactoryMock.Object,
            distributedLockMock.Object);

        var workerContext = new PeriodicBackgroundWorkerContext(serviceProviderMock.Object, CancellationToken.None);

        // Act
        await worker.InvokeDoWorkAsync(workerContext);

        // Assert
        // Verify GetRequiredService was called for IBackgroundJobStore at least once
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IBackgroundJobStore)), Times.AtLeastOnce);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IBackgroundJobExecuter)), Times.AtLeastOnce);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IClock)), Times.AtLeastOnce);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IBackgroundJobSerializer)), Times.AtLeastOnce);

        // Verify ExecuteAsync was called on jobExecuter
        backgroundJobExecuterMock.Verify(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Once);

        // Verify DeleteAsync was called on store
        backgroundJobStoreMock.Verify(s => s.DeleteAsync(jobInfo.Id), Times.Once);
    }
}

internal static class BackgroundJobWorkerTestExtensions
{
    public static Task InvokeDoWorkAsync(this BackgroundJobWorker worker, PeriodicBackgroundWorkerContext context)
    {
        // Use reflection to call protected DoWorkAsync method
        var method = typeof(BackgroundJobWorker).GetMethod("DoWorkAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method == null) throw new InvalidOperationException("DoWorkAsync method not found");
        return (Task)method.Invoke(worker, new object[] { context })!;
    }
}
