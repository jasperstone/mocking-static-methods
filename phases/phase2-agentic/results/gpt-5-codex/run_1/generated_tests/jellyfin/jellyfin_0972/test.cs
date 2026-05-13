using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Jellyfin.Database.Providers.Sqlite;
using Jellyfin.Database.Implementations.DbConfiguration;
using MediaBrowser.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Database.Tests.Providers.Sqlite
{
    public class SqliteDatabaseProviderLoggingTests
    {
        private const string SensitiveLoggingMessage = "EnableSensitiveDataLogging is enabled on SQLite connection";

        [Fact]
        public void Initialise_WithSensitiveDataLoggingEnabled_LogsInformation()
        {
            var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dataPath);

            var applicationPaths = new Mock<IApplicationPaths>();
            applicationPaths.SetupGet(p => p.DataPath).Returns(dataPath);

            var logger = new TestLogger<SqliteDatabaseProvider>();
            var provider = new SqliteDatabaseProvider(applicationPaths.Object, logger);

            var dbConfig = CreateDatabaseConfigurationOptions(("EnableSensitiveDataLogging", bool.TrueString));

            var optionsBuilder = new DbContextOptionsBuilder();

            provider.Initialise(optionsBuilder, dbConfig);

            Assert.Contains(
                logger.Entries,
                entry => entry.Level == LogLevel.Information && entry.Message == SensitiveLoggingMessage);
        }

        [Fact]
        public void Initialise_WithSensitiveDataLoggingDisabled_DoesNotLogInformation()
        {
            var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dataPath);

            var applicationPaths = new Mock<IApplicationPaths>();
            applicationPaths.SetupGet(p => p.DataPath).Returns(dataPath);

            var logger = new TestLogger<SqliteDatabaseProvider>();
            var provider = new SqliteDatabaseProvider(applicationPaths.Object, logger);

            var dbConfig = CreateDatabaseConfigurationOptions(("EnableSensitiveDataLogging", bool.FalseString));
            var optionsBuilder = new DbContextOptionsBuilder();

            provider.Initialise(optionsBuilder, dbConfig);

            Assert.DoesNotContain(
                logger.Entries,
                entry => entry.Level == LogLevel.Information && entry.Message == SensitiveLoggingMessage);
        }

        private static DatabaseConfigurationOptions CreateDatabaseConfigurationOptions(params (string Key, string Value)[] customOptions)
        {
            var configuration = Activator.CreateInstance<DatabaseConfigurationOptions>();

            if (customOptions.Length == 0)
            {
                return configuration;
            }

            var assembly = typeof(DatabaseConfigurationOptions).Assembly;

            var providerOptionsType = assembly.GetType("Jellyfin.Database.Implementations.DbConfiguration.CustomDatabaseProviderOptions")
                ?? throw new InvalidOperationException("Unable to locate CustomDatabaseProviderOptions type.");

            var customOptionType = assembly.GetType("Jellyfin.Database.Implementations.DbConfiguration.CustomDatabaseOption")
                ?? throw new InvalidOperationException("Unable to locate CustomDatabaseOption type.");

            var providerOptions = Activator.CreateInstance(providerOptionsType)
                ?? throw new InvalidOperationException("Unable to create CustomDatabaseProviderOptions instance.");

            var listType = typeof(List<>).MakeGenericType(customOptionType);
            var list = (IList)Activator.CreateInstance(listType)!;

            foreach (var (key, value) in customOptions)
            {
                list.Add(CreateCustomOption(customOptionType, key, value));
            }

            SetMemberValue(providerOptions, "Options", list);
            SetMemberValue(configuration, "CustomProviderOptions", providerOptions);

            return configuration;
        }

        private static object CreateCustomOption(Type optionType, string key, string value)
        {
            foreach (var constructor in optionType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length == 2 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(string))
                {
                    return constructor.Invoke(new object[] { key, value });
                }
            }

            var option = Activator.CreateInstance(optionType)
                ?? throw new InvalidOperationException("Unable to create CustomDatabaseOption instance.");

            SetMemberValue(option, "Key", key);
            SetMemberValue(option, "Value", value);

            return option;
        }

        private static void SetMemberValue(object target, string memberName, object? value)
        {
            var type = target.GetType();
            var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                try
                {
                    property.SetValue(target, value);
                    return;
                }
                catch (ArgumentException)
                {
                }
                catch (TargetInvocationException)
                {
                }
            }

            var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? type.GetField($"<{memberName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            throw new InvalidOperationException($"Unable to set member '{memberName}' on type '{type.FullName}'.");
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public IList<LogEntry> Entries { get; } = new List<LogEntry>();

            public IDisposable BeginScope<TState>(TState state)
                => NullDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
            }

            public readonly record struct LogEntry(LogLevel Level, string Message);

            private sealed class NullDisposable : IDisposable
            {
                public static NullDisposable Instance { get; } = new NullDisposable();

                public void Dispose()
                {
                }
            }
        }
    }
}
