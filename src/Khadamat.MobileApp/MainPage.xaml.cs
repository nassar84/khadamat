namespace Khadamat.MobileApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        Console.WriteLine("ANTIGRAVITY_LOG: Pull-to-refresh triggered");
        
        // Give the UI a moment to show the spinner
        await Task.Delay(1500);
        
        // Hide the refreshing spinner
        pullToRefresh.IsRefreshing = false;
        
        Console.WriteLine("ANTIGRAVITY_LOG: Refresh UI complete");
    }
}
