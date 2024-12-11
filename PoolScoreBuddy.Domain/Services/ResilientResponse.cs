namespace PoolScoreBuddy.Domain;

internal record ResilientResponse()
{
    public HttpResponseMessage Response { get; set; }
    public IEnumerable<string> KnownBadEndpoints { get; set; } = [];
}

