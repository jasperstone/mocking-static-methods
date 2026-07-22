using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        private class TestFolder : Folder
        {
            private List<BaseItem> _children = new List<BaseItem>();

            public override IEnumerable<BaseItem> Children
            {
                get => _children;
                set => _children = value?.ToList() ?? new List<BaseItem>();
            }

            // Expose the private GetActualChildrenDictionary method via reflection for testing
            public Dictionary<Guid, BaseItem> CallGetActualChildrenDictionary()
            {
                var method = typeof(Folder).GetMethod("GetActualChildrenDictionary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (Dictionary<Guid, BaseItem>)method.Invoke(this, null);
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

        [Fact]
        public void GetActualChildrenDictionary_LogsErrorOnDuplicateIds()
        {
            // Arrange
            var loggerCalled = false;
            string loggedMessage = null;
            string loggedPath = null;
            string loggedChildName = null;

            var testFolder = new TestFolder
            {
                Path = "/test/folder"
            };

            var duplicateId = Guid.NewGuid();

            var child1 = new TestBaseItem(duplicateId, "Child1", "/test/folder/child1");
            var child2 = new TestBaseItem(duplicateId, "Child2", "/test/folder/child2");

            testFolder.Children = new List<BaseItem> { child1, child2 };

            // Replace the static Logger with a test logger that captures LogError calls
            Folder.Logger = (ILogger<BaseItem>)(object)new TestLogger((logLevel, eventId, state, exception, formatter) =>
            {
                if (logLevel == LogLevel.Error)
                {
                    loggerCalled = true;
                    loggedMessage = formatter(state, exception);
                    if (state is IReadOnlyList<KeyValuePair<string, object>> props)
                    {
                        foreach (var kvp in props)
                        {
                            if (kvp.Key == "Path") loggedPath = kvp.Value?.ToString();
                            if (kvp.Key == "ChildName") loggedChildName = kvp.Value?.ToString();
                        }
                    }
                }
                return true;
            });

            // Act
            var dict = testFolder.CallGetActualChildrenDictionary();

            // Assert
            Assert.True(loggerCalled);
            Assert.Contains("Found folder containing items with duplicate id", loggedMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(testFolder.Path, loggedPath);
            Assert.Equal(child2.Path, loggedChildName);
            Assert.Single(dict);
            Assert.True(dict.ContainsKey(duplicateId));
            Assert.Equal(child1, dict[duplicateId]);
        }

        private class TestLogger : ILogger<BaseItem>
        {
            private readonly Func<LogLevel, EventId, object, Exception, Func<object, Exception, string>, bool> _logAction;

            public TestLogger(Func<LogLevel, EventId, object, Exception, Func<object, Exception, string>, bool> logAction)
            {
                _logAction = logAction;
            }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _logAction(logLevel, eventId, state, exception, (o, e) => formatter((TState)o, e));
            }
        }
    }
}
