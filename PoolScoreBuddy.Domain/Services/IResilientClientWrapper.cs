using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public interface IResilientClientWrapper
{
    Task<HttpResponseMessage?> FetchResponse(HttpClient client, ApiDto dto);
}