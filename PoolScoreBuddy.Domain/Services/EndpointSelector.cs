using System.Diagnostics;

namespace PoolScoreBuddy.Domain.Services;

internal static class EndpointSelector
{
    /// <summary>
    /// Selects an endpoint based on an affinity.
    /// We want requests from the same identifier to go to the same endpoint
    /// This balances the load, with an api instance chosen based on affinity (tournament) id
    /// </summary>
    /// <param name="affinityIdentifier"></param>
    /// <returns></returns>
    public static string? SelectEndpoint(IEnumerable<string> preferredEndpoints, IEnumerable<string> badEndpoints, string affinityIdentifier)
    {
        var goodEndpoints = preferredEndpoints.Where(p => !badEndpoints.Any(p2 => p2 == p)).ToList();

        if (goodEndpoints.Count == 0)
            return null;

        int index = GetAffinityIndex(affinityIdentifier, goodEndpoints.Count);

        return goodEndpoints[index];
    }

    private static int GetAffinityIndex(string candidate, int rangeCount)
    {
        int total = 0;

        foreach (char letter in candidate)
        {
            // Get the integral value of the character.
            int value = Convert.ToInt32(letter);
            total += value;
        }

        Debug.WriteLine($"mod value of {candidate} is {total % rangeCount}");

        return total % rangeCount;
    }
}
