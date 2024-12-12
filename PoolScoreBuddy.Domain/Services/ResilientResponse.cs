namespace PoolScoreBuddy.Domain;

internal record ResilientResponse()
{
    public HttpResponseMessage Response { get; set; } = null!;
    public IEnumerable<string> KnownBadEndpoints { get; set; } = [];
}

