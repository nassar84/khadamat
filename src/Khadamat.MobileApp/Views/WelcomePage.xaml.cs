using System;
using MauiApp = Microsoft.Maui.Controls.Application;
using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;

namespace Khadamat.MobileApp.Views
{
    public partial class WelcomePage : ContentPage
    {
        private readonly AppShell _shell;
        private readonly IAudioManager _audioManager;

        public AppShell AppShellInstance => _shell;

        public WelcomePage(AppShell shell, IAudioManager audioManager)
        {
            InitializeComponent();
            _shell = shell;
            _audioManager = audioManager;
            BindingContext = _shell.BindingContext;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try 
            {
                var player = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("startup_sound.mp3"));
                player.Play();
            }
            catch { /* File might be missing */ }
        }

        public AppShell GetShell() => _shell;

        private async void OnTermsClicked(object sender, EventArgs e)
        {
            // Navigate to terms page
            if (MauiApp.Current != null)
            {
                // We navigate to a container that loads the /terms route
                await Shell.Current.GoToAsync("//HomePage?route=terms");
            }
        }

        private async void OnStartClicked(object sender, EventArgs e)
        {
            // Transition to the main app shell
            if (MauiApp.Current != null)
            {
                // Play a small animation before switching
                await ((VisualElement)sender).ScaleTo(0.9, 100);
                await ((VisualElement)sender).ScaleTo(1.0, 100);
                
                MauiApp.Current.MainPage = _shell;
            }
        }
    }
}
