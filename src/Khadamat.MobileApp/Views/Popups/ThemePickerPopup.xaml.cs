using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace Khadamat.MobileApp.Views.Popups;

public partial class ThemePickerPopup : Popup
{
    // Fan themes: 6 colored buttons arranged in a semicircle
    private readonly (string Id, string Title, string Hex)[] _themes = new[]
    {
        ("default",   "\u0623\u0632\u0631\u0642",      "#2563eb"),   // Blue - matches [data-theme="default"]
        ("sunset",    "\u063a\u0631\u0648\u0628",      "#ea580c"),   // Orange-red
        ("ocean",     "\u0645\u062d\u064a\u0637",      "#0ea5e9"),   // Sky blue
        ("forest",    "\u063a\u0627\u0628\u0629",      "#10b981"),   // Green
        ("lavender",  "\u0644\u0627\u0641\u0646\u062f\u0631",    "#8b5cf6"),   // Purple
        ("royal",     "\u0645\u0644\u0643\u064a",      "#eab308")    // Gold
    };

    // Center button: off-white light/glass theme
    private const string CenterThemeId    = "glass";
    private const string CenterThemeTitle = "\u062e\u0641\u064a\u0641";
    private const string CenterThemeHex   = "#f1f5f9";
    private const string CenterBorderHex  = "#cbd5e1";

    private bool _isAnimated = false;
    private readonly VisualElement[] _themeElements;
    private VisualElement _centerElement = null!;

    public ThemePickerPopup()
    {
        InitializeComponent();
        
        _themeElements = new VisualElement[_themes.Length];
        
        double centerX = 200;
        double centerY = 160;
        
        // ── Build 6 fan buttons ─────────────────────────────────────────────
        for (int i = 0; i < _themes.Length; i++)
        {
            var theme = _themes[i];
            var btnContainer = new VerticalStackLayout
            {
                Spacing = 5,
                Opacity = 1,
                Scale = 0,
                TranslationX = centerX - 35,
                TranslationY = centerY
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
            
            AbsoluteLayout.SetLayoutBounds(btnContainer, new Rect(0, 0, 70, 80));
            AbsoluteLayout.SetLayoutFlags(btnContainer, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);
            ContainerLayout.Children.Add(btnContainer);
        }

        // ── Center button: white/light (glass) ──────────────────────────────
        var centerContainer = new VerticalStackLayout
        {
            Spacing = 5,
            Opacity = 0,
            Scale = 0,
            TranslationX = centerX - 35,
            TranslationY = centerY
        };

        var centerBtn = new Frame
        {
            HeightRequest = 50,
            WidthRequest = 50,
            CornerRadius = 25,
            BackgroundColor = Color.FromArgb(CenterThemeHex),
            HasShadow = true,
            Padding = 0,
            HorizontalOptions = LayoutOptions.Center,
            BorderColor = Color.FromArgb(CenterBorderHex)
        };

        var centerTap = new TapGestureRecognizer();
        centerTap.Tapped += async (s, e) =>
        {
            await centerBtn.ScaleTo(0.8, 100);
            await centerBtn.ScaleTo(1.0, 100);
            Close(CenterThemeId);
        };
        centerBtn.GestureRecognizers.Add(centerTap);

        var centerLabel = new Label
        {
            Text = CenterThemeTitle,
            FontSize = 10,
            TextColor = Color.FromArgb("#475569"),
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        centerContainer.Children.Add(centerBtn);
        centerContainer.Children.Add(centerLabel);

        AbsoluteLayout.SetLayoutBounds(centerContainer, new Rect(0, 0, 70, 80));
        AbsoluteLayout.SetLayoutFlags(centerContainer, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.None);
        ContainerLayout.Children.Add(centerContainer);
        _centerElement = centerContainer;

        this.Opened += OnPopupOpened;
        
        // Safety fallback: trigger animation if Opened event doesn't fire
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () => OnPopupOpened(null, null));
    }

    private async void OnPopupOpened(object? sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs? e)
    {
        if (_isAnimated) return;
        _isAnimated = true;
        
        double radius = 110;
        double centerX = 200; 
        double centerY = 160; 
        
        double angleStep = Math.PI / (_themes.Length - 1);
        
        var tasks = new Task[_themes.Length + 1];
        for (int i = 0; i < _themes.Length; i++)
        {
            double angle = Math.PI - (i * angleStep);
            
            double targetX = centerX + radius * Math.Cos(angle) - 35; 
            double targetY = centerY - radius * Math.Sin(angle) - 45; 
            
            tasks[i] = AnimateElementAsync(_themeElements[i], targetX, targetY, i * 60);
        }
        
        // Center button animates last to the center of the arc
        double centerTargetX = centerX - 35;
        double centerTargetY = centerY - 50;
        tasks[_themes.Length] = AnimateCenterAsync(_centerElement, centerTargetX, centerTargetY, _themes.Length * 60);
        
        await Task.WhenAll(tasks);
    }
    
    private async Task AnimateElementAsync(VisualElement element, double targetX, double targetY, int delay)
    {
        await Task.Delay(delay);
        _ = element.ScaleTo(1, 400, Easing.SpringOut);
        await element.TranslateTo(targetX, targetY, 600, Easing.SpringOut);
    }

    private async Task AnimateCenterAsync(VisualElement element, double targetX, double targetY, int delay)
    {
        await Task.Delay(delay);
        _ = element.FadeTo(1, 350);
        _ = element.ScaleTo(1, 400, Easing.SpringOut);
        await element.TranslateTo(targetX, targetY, 600, Easing.SpringOut);
    }
}
