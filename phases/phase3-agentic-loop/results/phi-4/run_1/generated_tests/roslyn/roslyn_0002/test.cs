   public class BuildHostProcessManager : IAsyncDisposable
   {
       private readonly IMonoMSBuildDiscoveryWrapper _monoMSBuildDiscoveryWrapper;

       public BuildHostProcessManager(
           ImmutableDictionary<string, string>? globalMSBuildProperties = null,
           IBinLogPathProvider? binaryLogPathProvider = null,
           ILoggerFactory? loggerFactory = null,
           IMonoMSBuildDiscoveryWrapper monoMSBuildDiscoveryWrapper = null)
       {
           _globalMSBuildProperties = globalMSBuildProperties ?? ImmutableDictionary<string, string>.Empty;
           _binaryLogPathProvider = binaryLogPathProvider;
           _loggerFactory = loggerFactory;
           _logger = loggerFactory?.CreateLogger<BuildHostProcessManager>();
           _monoMSBuildDiscoveryWrapper = monoMSBuildDiscoveryWrapper ?? new MonoMSBuildDiscoveryWrapper();
       }

       // Existing methods...
   }
   