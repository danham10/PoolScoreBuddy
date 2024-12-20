namespace PoolScoreBuddy;

public class Alert : IAlert
{
    public async Task Show(string title, string message, string buttonText)
    {
        await Application.Current!.MainPage!.DisplayAlert(title, message, buttonText);
    }
}

public interface IAlert
{
    public Task Show(string title, string message, string buttonText);
}
