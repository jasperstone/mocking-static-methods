using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Controller.SyncPlay.GroupStates;
using MediaBrowser.Model.SyncPlay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.SyncPlay.Tests.GroupStates
{
    public class WaitingGroupStateTests
    {
        private class TestLogger<T> : ILogger<T>
        {
            public LogLevel? LastLogLevel { get; private set; }
            public string LastMessage { get; private set; }
            public object[] LastArgs { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LastMessage = formatter(state, exception);
                if (state is IReadOnlyList<KeyValuePair<string, object>> props)
                {
                    var argsList = new System.Collections.Generic.List<object>();
                    foreach (var kvp in props)
                    {
                        if (kvp.Key != "{OriginalFormat}")
                            argsList.Add(kvp.Value);
                    }
                    LastArgs = argsList.ToArray();
                }
            }
        }

        [Fact]
        public void SessionLeaving_ResumePlayingTrue_LogsDebugAndSetsPlayingState()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var testLogger = new TestLogger<WaitingGroupState>();
            loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(testLogger);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = true
            };

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>(MockBehavior.Strict, null, null);
            sessionMock.SetupGet(s => s.Id).Returns("session1");
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            IGroupState capturedState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => capturedState = state);

            var cancellationToken = CancellationToken.None;

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionMock.Object, cancellationToken);

            // Assert
            Assert.Equal(LogLevel.Debug, testLogger.LastLogLevel);
            Assert.Contains("Session session1 left group", testLogger.LastMessage);
            Assert.NotNull(capturedState);
            Assert.IsType<PlayingGroupState>(capturedState);
        }

        [Fact]
        public void SessionLeaving_ResumePlayingFalse_LogsDebugAndSetsPausedState()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var testLogger = new TestLogger<WaitingGroupState>();
            loggerFactoryMock.Setup(f => f.CreateLogger<WaitingGroupState>()).Returns(testLogger);

            var waitingState = new WaitingGroupState(loggerFactoryMock.Object)
            {
                ResumePlaying = false
            };

            var contextMock = new Mock<IGroupStateContext>();
            var sessionMock = new Mock<SessionInfo>(MockBehavior.Strict, null, null);
            sessionMock.SetupGet(s => s.Id).Returns("session2");
            var groupId = Guid.NewGuid();
            contextMock.Setup(c => c.GroupId).Returns(groupId);
            contextMock.Setup(c => c.IsBuffering()).Returns(false);

            IGroupState capturedState = null;
            contextMock.Setup(c => c.SetState(It.IsAny<IGroupState>()))
                .Callback<IGroupState>(state => capturedState = state);

            var cancellationToken = CancellationToken.None;

            // Act
            waitingState.SessionLeaving(contextMock.Object, GroupStateType.Waiting, sessionMock.Object, cancellationToken);

            // Assert
            Assert.Equal(LogLevel.Debug, testLogger.LastLogLevel);
            Assert.Contains("Session session2 left group", testLogger.LastMessage);
            Assert.NotNull(capturedState);
            Assert.IsType<PausedGroupState>(capturedState);
        }
    }
}
