using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace eShop.Tests.Shared
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public void AddMigration_WithSeederType_RegistersSeederAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            InvokeTwoTypeAddMigration(services);

            // Assert
            var seederDescriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IDbSeeder<TestDbContext>)));
            Assert.Equal(ServiceLifetime.Scoped, seederDescriptor.Lifetime);
            Assert.Equal(typeof(TestSeeder), seederDescriptor.ImplementationType);
        }

        [Fact]
        public async Task AddMigration_WithSeederType_SeederDelegateResolvesAndRunsSeederUsingServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            InvokeTwoTypeAddMigration(services);

            var hostedServiceDescriptor = Assert.Single(
                services.Where(d =>
                    d.ServiceType == typeof(IHostedService) &&
                    d.ImplementationFactory is not null &&
                    d.ImplementationFactory.Method.DeclaringType?.FullName == "Microsoft.AspNetCore.Hosting.MigrateDbContextExtensions"));

            var dummyProvider = new ServiceCollection().BuildServiceProvider();
            var hostedService = Assert.IsAssignableFrom<IHostedService>(hostedServiceDescriptor.ImplementationFactory!(dummyProvider));

            var seederDelegate = GetSeederDelegate(hostedService);

            using var context = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().Options);
            var testSeeder = new TestSeeder();
            var recordingProvider = new RecordingServiceProvider(testSeeder);

            // Act
            await seederDelegate(context, recordingProvider);

            // Assert
            var requestedType = Assert.Single(recordingProvider.RequestedServices);
            Assert.Equal(typeof(IDbSeeder<TestDbContext>), requestedType);
            Assert.Same(context, testSeeder.SeededContext);
        }

        private static Func<TestDbContext, IServiceProvider, Task> GetSeederDelegate(IHostedService hostedService)
        {
            var seederField = hostedService.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(f => typeof(Func<TestDbContext, IServiceProvider, Task>).IsAssignableFrom(f.FieldType));

            return (Func<TestDbContext, IServiceProvider, Task>)seederField.GetValue(hostedService)!;
        }

        private static void InvokeTwoTypeAddMigration(IServiceCollection services)
        {
            var extensionsType = typeof(IDbSeeder<>).Assembly.GetType("Microsoft.AspNetCore.Hosting.MigrateDbContextExtensions", throwOnError: true)!;

            var method = extensionsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "AddMigration" && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 1);

            var generic = method.MakeGenericMethod(typeof(TestDbContext), typeof(TestSeeder));
            generic.Invoke(null, new object[] { services });
        }

        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options)
                : base(options)
            {
            }
        }

        private sealed class TestSeeder : IDbSeeder<TestDbContext>
        {
            public TestDbContext? SeededContext { get; private set; }

            public Task SeedAsync(TestDbContext context)
            {
                SeededContext = context;
                return Task.CompletedTask;
            }
        }

        private sealed class RecordingServiceProvider : IServiceProvider
        {
            private readonly object _seeder;

            public RecordingServiceProvider(object seeder)
            {
                _seeder = seeder;
            }

            public List<Type> RequestedServices { get; } = new();

            public object? GetService(Type serviceType)
            {
                RequestedServices.Add(serviceType);

                if (serviceType == typeof(IDbSeeder<TestDbContext>))
                {
                    return _seeder;
                }

                return null;
            }
        }
    }
}
