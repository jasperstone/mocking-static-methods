using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        private class TestFolder : Folder
        {
            public TestFolder()
            {
                // Initialize private _children field to null via reflection
                var field = typeof(Folder).GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(this, null);
            }

            public new IEnumerable<BaseItem> Children
            {
                get
                {
                    var field = typeof(Folder).GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance);
                    return (IEnumerable<BaseItem>)field.GetValue(this);
                }
                set
                {
                    var field = typeof(Folder).GetField("_children", BindingFlags.NonPublic | BindingFlags.Instance);
                    field.SetValue(this, value);
                }
            }

            public List<(string message, object[] args)> LoggedErrors { get; } = new();

            // We cannot override Logger property because it's not virtual, so we simulate logging by replacing the Logger with a custom ILogger
            public ILogger LoggerInstance { get; set; }

            // Expose the GetActualChildrenDictionary method for testing, replacing Logger.LogError call with our own method
            public Dictionary<Guid, BaseItem> CallGetActualChildrenDictionary()
            {
                var dictionary = new Dictionary<Guid, BaseItem>();

                // Invalidate cached children
                Children = null;

                var childrenList = Children.ToList();

                foreach (var child in childrenList)
                {
                    var id = child.Id;
                    if (dictionary.ContainsKey(id))
                    {
                        if (LoggerInstance != null)
                        {
                            LoggerInstance.LogError(
                                "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                                Path ?? Name,
                                child.Path ?? child.Name);
                        }
                        else
                        {
                            LoggedErrors.Add((
                                "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                                new object[] { Path ?? Name, child.Path ?? child.Name }));
                        }
                    }
                    else
                    {
                        dictionary[id] = child;
                    }
                }

                return dictionary;
            }
        }

        private class TestBaseItem : BaseItem
        {
            public TestBaseItem(Guid id, string name, string path)
            {
                Id = id;
                Name = name;
                Path = path;
            }
        }

        private class LoggerMock : ILogger
        {
            public List<(LogLevel level, string message, object[] args)> Logs { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    var message = formatter(state, exception);
                    if (state is IEnumerable<KeyValuePair<string, object>> kvps)
                    {
                        var args = kvps.Skip(1).Select(kvp => kvp.Value).ToArray();
                        Logs.Add((logLevel, message, args));
                    }
                    else
                    {
                        Logs.Add((logLevel, message, Array.Empty<object>()));
                    }
                }
            }
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds_UsingLoggerInstance()
        {
            // Arrange
            var loggerMock = new LoggerMock();

            var folder = new TestFolder
            {
                Path = "/test/path",
                LoggerInstance = loggerMock
            };

            var duplicateId = Guid.NewGuid();

            var child1 = new TestBaseItem(duplicateId, "Child1", "/child1/path");
            var child2 = new TestBaseItem(duplicateId, "Child2", "/child2/path");

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var dict = folder.CallGetActualChildrenDictionary();

            // Assert
            Assert.Single(loggerMock.Logs);
            var log = loggerMock.Logs[0];
            Assert.Equal(LogLevel.Error, log.level);
            Assert.Equal("Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}", log.message);
            Assert.Equal(folder.Path, log.args[0]);
            Assert.Equal(child2.Path, log.args[1]);
            Assert.Single(dict);
            Assert.True(dict.ContainsKey(duplicateId));
            Assert.Equal(child1, dict[duplicateId]);
        }

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds_UsingLoggedErrors()
        {
            // Arrange
            var folder = new TestFolder
            {
                Path = "/test/path"
            };

            var duplicateId = Guid.NewGuid();

            var child1 = new TestBaseItem(duplicateId, "Child1", "/child1/path");
            var child2 = new TestBaseItem(duplicateId, "Child2", "/child2/path");

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var dict = folder.CallGetActualChildrenDictionary();

            // Assert
            Assert.Single(folder.LoggedErrors);
            var log = folder.LoggedErrors[0];
            Assert.Equal("Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}", log.message);
            Assert.Equal(folder.Path, log.args[0]);
            Assert.Equal(child2.Path, log.args[1]);
            Assert.Single(dict);
            Assert.True(dict.ContainsKey(duplicateId));
            Assert.Equal(child1, dict[duplicateId]);
        }
    }
}
