using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class DeltaLogTests
    {
        [Fact]
        public void AsyncFlushPageToDeviceCallback_ErrorCodeNonZero_LogsError()
        {
            var logger = new CollectingLogger();
            var deltaLog = CreateDeltaLogWithLogger(logger);
            var context = CreatePageAsyncFlushResult(count: 2);

            InvokeAsyncFlushPageToDeviceCallback(deltaLog, errorCode: 123, numBytes: 0, context);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Error, entry.LogLevel);
            Assert.Equal("AsyncFlushPageToDeviceCallback error: 123", entry.Message);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_ErrorCodeZero_DoesNotLogError()
        {
            var logger = new CollectingLogger();
            var deltaLog = CreateDeltaLogWithLogger(logger);
            var context = CreatePageAsyncFlushResult(count: 2);

            InvokeAsyncFlushPageToDeviceCallback(deltaLog, errorCode: 0, numBytes: 0, context);

            Assert.Empty(logger.Entries);
        }

        private static DeltaLog CreateDeltaLogWithLogger(CollectingLogger logger)
        {
            var deltaLog = (DeltaLog)FormatterServices.GetUninitializedObject(typeof(DeltaLog));
            SetPrivateField(deltaLog, "completedSemaphore", new SemaphoreSlim(0));
            SetPrivateField(deltaLog, "issuedFlush", 2);
            SetPrivateField(deltaLog, "disposed", false);
            SetPrivateField(deltaLog, "logger", logger);
            return deltaLog;
        }

        private static void InvokeAsyncFlushPageToDeviceCallback(DeltaLog deltaLog, uint errorCode, uint numBytes, object context)
        {
            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", BindingFlags.Instance | BindingFlags.NonPublic);
            method!.Invoke(deltaLog, new object[] { errorCode, numBytes, context });
        }

        private static object CreatePageAsyncFlushResult(int count)
        {
            var assembly = typeof(DeltaLog).Assembly;
            var emptyType = assembly.GetType("Tsavorite.core.Empty", throwOnError: true)!;
            var genericType = assembly.GetType("Tsavorite.core.PageAsyncFlushResult`1", throwOnError: true)!.MakeGenericType(emptyType);
            var instance = Activator.CreateInstance(genericType)!;

            var countField = genericType.GetField("count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            countField!.SetValue(instance, count);

            return instance;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var currentType = target.GetType();
            while (currentType != null)
            {
                var field = currentType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }

                currentType = currentType.BaseType;
            }

            throw new InvalidOperationException($"Field '{fieldName}' not found on {target.GetType()}.");
        }

        private sealed class CollectingLogger : ILogger
        {
            private readonly List<LogEntry> entries = new();

            public IReadOnlyList<LogEntry> Entries => entries;

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                entries.Add(new LogEntry(logLevel, message, exception));
            }

            public record LogEntry(LogLevel LogLevel, string Message, Exception Exception);
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose()
            {
            }
        }
    }
}
