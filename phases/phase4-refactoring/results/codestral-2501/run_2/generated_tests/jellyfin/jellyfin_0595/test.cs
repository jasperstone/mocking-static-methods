using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Serialization;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.SystemBackupService;
using Jellyfin.Server.Migrations.Stages;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.SystemBackupService;
using MediaBrowser.Model.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Migrations;

/// <summary>
/// Handles Migration of the Jellyfin data structure.
/// </summary>
public class JellyfinMigrationService
{
    private const string DbFilename = "library.db";
    private readonly IDbContextFactory<JellyfinDbContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IStartupLogger _startupLogger;
    private readonly IBackupService? _backupService;
    private readonly IJellyfinDatabaseProvider? _jellyfinDatabaseProvider;
    private readonly IApplicationPaths _applicationPaths;
    private (string? LibraryDb, string? JellyfinDb, BackupManifestDto? FullBackup) _backupKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="JellyfinMigrationService"/> class.
    /// </summary>
    /// <param name="dbContextFactory">Provides access to the jellyfin database.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="startupLogger">The startup logger for Startup UI intigration.</param>
    /// <param name="applicationPaths">Application paths for library.db backup.</param>
    /// <param name="backupService">The jellyfin backup service.</param>
    /// <param name="jellyfinDatabaseProvider">The jellyfin database provider.</param>
    public JellyfinMigrationService(
        IDbContextFactory<JellyfinDbContext> dbContextFactory,
        ILoggerFactory loggerFactory,
        IStartupLogger<JellyfinMigrationService> startupLogger,
        IApplicationPaths applicationPaths,
        IBackupService? backupService = null,
        IJellyfinDatabaseProvider? jellyfinDatabaseProvider = null)
    {
        _dbContextFactory = dbContextFactory;
        _loggerFactory = loggerFactory;
        _startupLogger = startupLogger;
        _backupService = backupService;
        _jellyfinDatabaseProvider = jellyfinDatabaseProvider;
        _applicationPaths = applicationPaths;
#pragma warning disable CS0618 // Type or member is obsolete
        Migrations = [.. typeof(IMigrationRoutine).Assembly.GetTypes().Where(e => typeof(IMigrationRoutine).IsAssignableFrom(e) || typeof(IAsyncMigrationRoutine).IsAssignableFrom(e))
            .Select(e => (Type: e, Metadata: e.GetCustomAttribute<JellyfinMigrationAttribute>(), Backup: e.GetCustomAttributes<JellyfinMigrationBackupAttribute>()))
            .Where(e => e.Metadata is not null)
            .GroupBy(e => e.Metadata!.Stage)
            .Select(f =>
            {
                var stage = new MigrationStage(f.Key);
                foreach (var item in f)
                {
                    JellyfinMigrationBackupAttribute? backupMetadata = null;
                    if (item.Backup?.Any() == true)
                    {
                        backupMetadata = item.Backup.Aggregate(MergeBackupAttributes);
                    }

                    stage.Add(new(item.Type, item.Metadata!, backupMetadata));
                }

                return stage;
            })];
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private interface IInternalMigration
    {
        Task PerformAsync(IStartupLogger logger);
    }

    private HashSet<MigrationStage> Migrations { get; set; }

    public async Task CheckFirstTimeRunOrMigration(IApplicationPaths appPaths, StartupOptions startupOptions)
    {
        var logger = _startupLogger.With(_loggerFactory.CreateLogger<JellyfinMigrationService>()).BeginGroup($"Migration Startup");
        logger.LogInformation("Initialise Migration service.");
        var xmlSerializer = new MyXmlSerializer();
        var serverConfig = File.Exists(appPaths.SystemConfigurationFilePath)
            ? (ServerConfiguration)xmlSerializer.DeserializeFromFile(typeof(ServerConfiguration), appPaths.SystemConfigurationFilePath)!
            : new ServerConfiguration();
        if (!serverConfig.IsStartupWizardCompleted || startupOptions.StartupMode is Configuration.StartupMode.SeedSystem)
        {
            logger.LogInformation("System initialization detected. Seed data. Startup mode is: {StartupMode}", startupOptions.StartupMode ?? Configuration.StartupMode.MediaServer);
            var flatApplyMigrations = Migrations.SelectMany(e => e.Where(f => !f.Metadata.RunMigrationOnSetup)).ToArray();

            var dbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
            await using (dbContext.ConfigureAwait(false))
            {
                var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as IRelationalDatabaseCreator
                    ?? throw new InvalidOperationException("Jellyfin does only support relational databases.");
                if (!await databaseCreator.ExistsAsync().ConfigureAwait(false))
                {
                    await databaseCreator.CreateAsync().ConfigureAwait(false);
                }

                var historyRepository = dbContext.GetService<IHistoryRepository>();

                await historyRepository.CreateIfNotExistsAsync().ConfigureAwait(false);
                var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync().ConfigureAwait(false);
                var startupScripts = flatApplyMigrations
                    .Where(e => !appliedMigrations.Any(f => f != e.BuildCodeMigrationId()))
                    .Select(e => (Migration: e.Metadata, Script: historyRepository.GetInsertScript(new HistoryRow(e.BuildCodeMigrationId(), GetJellyfinVersion()))))
                    .ToArray();
                foreach (var item in startupScripts)
                {
                    logger.LogInformation("Seed migration {Key}-{Name}.", item.Migration.Key, item.Migration.Name);
                    await dbContext.Database.ExecuteSqlRawAsync(item.Script).ConfigureAwait(false);
                }
            }

            logger.LogInformation("Migration system initialisation completed.");
        }
        else
        {
            // migrate any existing migration.xml files
            var migrationConfigPath = Path.Join(appPaths.ConfigurationDirectoryPath, "migrations.xml");
            var migrationOptions = File.Exists(migrationConfigPath)
                 ? (MigrationOptions)xmlSerializer.DeserializeFromFile(typeof(MigrationOptions), migrationConfigPath)!
                 : null;
            if (migrationOptions is not null && migrationOptions.Applied.Count > 0)
            {
                logger.LogInformation("Old migration style migration.xml detected. Migrate now.");
                try
                {
                    var dbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
                    await using (dbContext.ConfigureAwait(false))
                    {
                        var historyRepository = dbContext.GetService<IHistoryRepository>();
                        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync().ConfigureAwait(false);
                        var lastOldAppliedMigration = Migrations
                            .SelectMany(e => e.Where(e => e.Metadata.Key is not null)) // only consider migrations that have the key set as its the reference marker for legacy migrations.
          

[... truncated, file is 23754 chars total ...]
