using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Metrics
{
    public class GarnetServerMonitorTests
    {
        private class TestOptions
        {
            public bool LatencyMonitor { get; set; }
        }

        private class TestGarnetServerMonitor : IDisposable
        {
            public IDictionary<InfoMetricsType, bool> ResetEventFlags { get; }
            public IList<object> Servers { get; }
            public Mock<ILogger> LoggerMock { get; }
            public GarnetServerMonitor Monitor { get; }

            public TestGarnetServerMonitor()
            {
                ResetEventFlags = new Dictionary<InfoMetricsType, bool>();
                Servers = new List<object>();
                LoggerMock = new Mock<ILogger>();

                Monitor = new GarnetServerMonitor(
                    servers: Servers,
                    globalMetrics: new GlobalMetrics(),
                    storeWrapper: new StoreWrapper(),
                    opts: new MonitorOptions(),
                    loggerFactory: Mock.Of<ILoggerFactory>(_ => _.CreateLogger(It.IsAny<string>()) == LoggerMock.Object),
                    cancellationToken: default,
                    sessionManager: null,
                    metricsCollector: null,
                    resetEventFlags: ResetEventFlags,
                    resetLatencyMetrics: new Dictionary<string, bool>());
            }

            public void Dispose()
            {
                Monitor.Dispose();
            }
        }

        [Fact]
        public void Update_ShouldLogInformation_WhenCommandStatsResetIsRequested()
        {
            using var fixture = new TestGarnetServerMonitor();
            fixture.ResetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            fixture.Monitor.Update();

            fixture.LoggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString() == "Resetting command stats"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    internal enum InfoMetricsType
    {
        STATS,
        COMMANDSTATS
    }

    internal class GlobalMetrics
    {
        public GlobalCommandStats globalCommandStats { get; set; } = new();
        public GlobalCommandStats historyCommandStats { get; set; } = new();
    }

    internal class GlobalCommandStats
    {
        public void Reset() { }
    }

    internal class StoreWrapper
    {
        public void ResetRevivificationStats() { }
    }

    internal class MonitorOptions { }

    internal class GarnetServerMonitor : IDisposable
    {
        private readonly IList<object> _servers;
        private readonly GlobalMetrics _globalMetrics;
        private readonly StoreWrapper _storeWrapper;
        private readonly MonitorOptions _opts;
        private readonly ILogger _logger;
        private readonly IDictionary<InfoMetricsType, bool> _resetEventFlags;
        private readonly IDictionary<string, bool> _resetLatencyMetrics;

        public GarnetServerMonitor(
            IList<object> servers,
            GlobalMetrics globalMetrics,
            StoreWrapper storeWrapper,
            MonitorOptions opts,
            ILoggerFactory loggerFactory,
            System.Threading.CancellationToken cancellationToken,
            object sessionManager,
            object metricsCollector,
            IDictionary<InfoMetricsType, bool> resetEventFlags,
            IDictionary<string, bool> resetLatencyMetrics)
        {
            _servers = servers;
            _globalMetrics = globalMetrics;
            _storeWrapper = storeWrapper;
            _opts = opts;
            _logger = loggerFactory.CreateLogger<GarnetServerMonitor>();
            _resetEventFlags = resetEventFlags;
            _resetLatencyMetrics = resetLatencyMetrics;
        }

        public void Update()
        {
            if (_resetEventFlags.TryGetValue(InfoMetricsType.COMMANDSTATS, out var resetCommandStats) && resetCommandStats)
            {
                _logger?.LogInformation("Resetting command stats");
                _globalMetrics.globalCommandStats?.Reset();
                _globalMetrics.historyCommandStats?.Reset();
                _resetEventFlags[InfoMetricsType.COMMANDSTATS] = false;
            }
        }

        public void Dispose()
        {
        }
    }
}
