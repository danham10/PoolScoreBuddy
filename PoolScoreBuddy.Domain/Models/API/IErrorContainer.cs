namespace PoolScoreBuddy.Domain.Models.API;

internal interface IErrorContainer
{
    public string? Error { get; set; }
}