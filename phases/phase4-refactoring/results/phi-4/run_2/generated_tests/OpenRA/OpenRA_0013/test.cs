public class ServerListLogic
{
    private readonly IHttpService _httpService;

    public ServerListLogic(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public void RefreshServerList()
    {
        // Query in progress
        if (activeQuery)
            return;

        searchStatus = SearchStatus.Fetching;

        var queryURL = new HttpQueryBuilder(services.ServerList)
        {
            { "protocol", GameServer.ProtocolVersion },
            { "engine", Game.EngineVersion },
            { "mod", Game.ModData.Manifest.Id },
            { "version", Game.ModData.Manifest.Metadata.Version }
        }.ToString();

        Task.Run(async () =>
        {
            List<GameServer> games = null;
            activeQuery = true;

            try
            {
                var httpResponseMessage = await _httpService.GetAsync(queryURL);
                var result = await httpResponseMessage.Content.ReadAsStreamAsync();

                var yaml = MiniYaml.FromStream(result, queryURL);
                games = new List<GameServer>();
                foreach (var node in yaml)
                {
                    try
                    {
                        var gs = new GameServer(node.Value);
                        if (gs.Address != null)
                            games.Add(gs);
                    }
                    catch
                    {
                        // Ignore any invalid games advertised.
                    }
                }
            }
            catch (Exception e)
            {
                searchStatus = SearchStatus.Failed;
                Log.Write("debug", $"Failed to query server list with exception: {e}");
            }

            // Continue with the rest of the method...
        });
    }
}
