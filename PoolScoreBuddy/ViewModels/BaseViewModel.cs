using CommunityToolkit.Mvvm.ComponentModel;

namespace PoolScoreBuddy.ViewModels;

public partial class BaseViewModel : ObservableObject
{

    [ObservableProperty]
    public bool isBusy;

    //[ObservableProperty]
    //public string? title;
}
