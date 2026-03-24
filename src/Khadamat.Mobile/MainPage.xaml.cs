using Microsoft.Maui.Controls;

namespace Khadamat.Mobile;

public partial class MainPage : ContentPage
{
    // For local testing on Android Emulator, use 10.0.2.2 instead of localhost
    // Once deployed, change this to the production website URL (e.g. https://khadamat.com)
    private readonly string _webUrl;

    public MainPage()
    {
        InitializeComponent();
        
        // Connect directly to the production website
        _webUrl = "https://jobsek.eis-dev.com";

        AppWebView.Source = _webUrl;
    }

    private void OnNavigating(object sender, WebNavigatingEventArgs e)
    {
        // Show indicator when we start navigating to a new page
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
    }

    private void OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        // Hide indicators when the page finishes loading
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        
        // Stop the pull-to-refresh animation if it was active
        AppRefreshView.IsRefreshing = false;
    }

    private void OnRefresh(object sender, EventArgs e)
    {
        // Reload the current page in the WebView
        AppWebView.Reload();
    }

    protected override bool OnBackButtonPressed()
    {
        // If the WebView has history, go back within the website
        if (AppWebView.CanGoBack)
        {
            AppWebView.GoBack();
            return true; // Handle the event (don't close the app)
        }

        // If no more history, allow the default behavior (close the app)
        return base.OnBackButtonPressed();
    }
}
