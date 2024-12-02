namespace PoolScoreBuddy.Models;

using System;

public class APIServerException : Exception
{
    public APIServerException()
    {
    }

    public APIServerException(string message)
        : base(message)
    {
    }

    public APIServerException(string message, Exception inner)
        : base(message, inner)
    {
    }
}