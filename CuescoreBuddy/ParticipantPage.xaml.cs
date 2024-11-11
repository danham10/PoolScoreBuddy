namespace CuescoreBuddy.Views;

public partial class ParticipantPage : ContentPage
{
	public ParticipantPage(ParticipantViewModel participantViewModel)
	{
		InitializeComponent();

        BindingContext = participantViewModel;
    }
}