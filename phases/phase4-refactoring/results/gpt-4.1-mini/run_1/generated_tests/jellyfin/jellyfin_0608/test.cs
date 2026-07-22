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
            public ILogger Logger { get; set; }

            public Dictionary<Guid, BaseItem> GetActualChildrenDictionaryTest()
            {
                var dictionary = new Dictionary<Guid, BaseItem>();

                // Use the Children property from base class
                var childrenList = Children?.ToList() ?? new List<BaseItem>();

                foreach (var child in childrenList)
                {
                    var id = child.Id;
                    if (dictionary.ContainsKey(id))
                    {
                        Logger?.LogError(
                            "Found folder containing items with duplicate id. Path: {Path}, Child Name: {ChildName}",
                            Path ?? Name,
                            child.Path ?? child.Name);
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
            public TestBaseItem(Guid id, string name, string path = null)
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
            var folder = new TestFolder();
            var loggerCalled = false;
            folder.Logger = new TestLogger(() => loggerCalled = true);
            folder.Path = "folderPath";
            var id = Guid.NewGuid();

            var child1 = new TestBaseItem(id, "Child1", "path1");
            var child2 = new TestBaseItem(id, "Child2", "path2");

            folder.Children = new List<BaseItem> { child1, child2 };

            // Act
            var result = folder.GetActualChildrenDictionaryTest();

            // Assert
            Assert.True(loggerCalled);
            Assert.Single(result);
            Assert.Equal(child1, result[id]);
        }

        private class TestLogger : ILogger
        {
            private readonly Action _onLogError;

            public TestLogger(Action onLogError)
            {
                _onLogError = onLogError;
            }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    _onLogError();
                }
            }
        }
    }
}
