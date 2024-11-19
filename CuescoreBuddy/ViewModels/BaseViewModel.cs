using CommunityToolkit.Mvvm.ComponentModel;

namespace CuescoreBuddy.ViewModels;

public partial class BaseViewModel : ObservableObject
{

    [ObservableProperty]
    public bool isBusy;

    string title = string.Empty;
    public string Title
    {
        get { return title; }
        set { SetProperty(ref title, value); }
    }

    public BaseViewModel()
    {
        //DataStore = ServiceHelper.GetService<IDataStore<Item>>();
    }

}
