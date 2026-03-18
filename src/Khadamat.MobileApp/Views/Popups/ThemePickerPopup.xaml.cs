using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace Khadamat.MobileApp.Views.Popups;

public partial class ThemePickerPopup : Popup
{
    private readonly (string Id, string Title, string Hex)[] _themes = new[]
    {
        ("default", "الافتراضي", "#6366f1"),
        ("sunset", "الغروب", "#ea580c"),
        ("ocean", "المحيط", "#0ea5e9"),
        ("forest", "الغابة", "#10b981"),
        ("lavender", "اللافندر", "#8b5cf6"),
        ("royal", "الملكي", "#eab308")
    };

    private bool _isAnimated = false;
    private readonly VisualElement[] _themeElements;

    public ThemePickerPopup()
    {
        InitializeComponent();
        
        _themeElements = new VisualElement[_themes.Length];
        
        double centerX = 170; // Half of 340 container width
        double centerY = 240; // Bottom anchoring for semi circle
        
        for (int i = 0; i < _themes.Length; i++)
        {
            var theme = _themes[i];
            var btnContainer = new VerticalStackLayout
            {
                Spacing = 5,
                Opacity = 0,
                Scale = 0.5,
                // Start everything at the bottom center to fan out
                TranslationX = centerX - 35, // 35 is half of 70 width
                TranslationY = centerY - 35
            };
            
            var btn = new Frame
            {
                HeightRequest = 56,
                WidthRequest = 56,
                CornerRadius = 28,
                BackgroundColor = Color.FromArgb(theme.Hex),
                HasShadow = true,
                Padding = 0,
                HorizontalOptions = LayoutOptions.Center
            };
            
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                await btn.ScaleTo(0.8, 100);
                await btn.ScaleTo(1.0, 100);
                Close(theme.Id);
            };
            btn.GestureRecognizers.Add(tapGesture);
            
            var label = new Label
            {
                Text = theme.Title,
                FontSize = 10,
                TextColor = Color.FromArgb("#475569"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            
            btnContainer.Children.Add(btn);
            btnContainer.Children.Add(label);
            
            _themeElements[i] = btnContainer;
            
            // Layout bounds fixed at 0,0 since we move them via Translation
            AbsoluteLayout.SetLayoutBounds(btnContainer, new Rect(0, 0, 70, 80));
            AbsoluteLayout.SetLayoutFlags(btnContainer, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);
            
            ContainerLayout.Children.Add(btnContainer);
        }

        this.Opened += OnPopupOpened;
    }

    private async void OnPopupOpened(object? sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e)
    {
        if (_isAnimated) return;
        _isAnimated = true;
        
        double radius = 110;
        double centerX = 170; 
        double centerY = 150; // Adjusted for the visible part of the semicircle
        
        // Distribution: From PI around to 0
        double angleStep = Math.PI / (_themes.Length - 1);
        
        var tasks = new Task[_themes.Length];
        for (int i = 0; i < _themes.Length; i++)
        {
            // Distribution: from PI to 0
            double angle = Math.PI - (i * angleStep);
            
            double targetX = centerX + radius * Math.Cos(angle) - 35; 
            double targetY = centerY - radius * Math.Sin(angle) - 40; 
            
            tasks[i] = AnimateElementAsync(_themeElements[i], targetX, targetY, i * 60);
        }
        
        await Task.WhenAll(tasks);
    }
    
    private async Task AnimateElementAsync(VisualElement element, double targetX, double targetY, int delay)
    {
        await Task.Delay(delay);
        
        // Make element visible
        _ = element.FadeTo(1, 400, Easing.CubicOut);
        _ = element.ScaleTo(1, 400, Easing.SpringOut);
        
        // Move element to its arc position
        await element.TranslateTo(targetX, targetY, 600, Easing.SpringOut);
    }
}
