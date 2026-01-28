using System;
using Khadamat.BlazorUI.Helpers;

namespace Khadamat.BlazorUI.State;

public class AppState
{
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { if (_isLoading != value) { _isLoading = value; NotifyStateChanged(); } }
    }

    private string? _userToken;
    public string? UserToken 
    { 
        get => _userToken; 
        set { _userToken = value; NotifyStateChanged(); } 
    }

    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    
    private string _userImageUrl = string.Empty;
    public string UserImageUrl 
    { 
        get => string.IsNullOrEmpty(_userImageUrl) ? DefaultImages.DefaultAvatar : _userImageUrl;
        set => _userImageUrl = value;
    }
    public bool IsProvider { get; set; }
    public int NotificationCount { get; set; } = 3;

    // Global Settings
    public string AppName { get; set; } = "خدمات";
    public string AppLogo { get; set; } = "";
    public string PrimaryColor { get; set; } = "#6366f1";
    public string SecondaryColor { get; set; } = "#a855f7";
   // UI State
    public bool IsSidebarOpen { get; set; }
    public bool IsProviderMode { get; set; } // If true, show dashboard. If false, show client UI.
    
    // Profile Information
    public int? CityId { get; set; }
    public int? GovernorateId { get; set; }
    public string? PhoneNumber { get; set; }
    
    public bool IsProfileComplete => CityId.HasValue && !string.IsNullOrEmpty(PhoneNumber);
    
    // Theme State
    private string _currentTheme = "default";
    public string CurrentTheme 
    {
        get => _currentTheme;
        private set 
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                NotifyStateChanged();
            }
        }
    }

    public void SetTheme(string themeName) => CurrentTheme = themeName;

    public void UpdateUserStatus(string userName, string userRole, bool isProvider, string imageUrl)
    {
        UserName = userName;
        UserRole = userRole;
        IsProvider = isProvider;
        UserImageUrl = imageUrl;
        NotifyStateChanged();
    }

    public void SetIsProvider(bool isProvider)
    {
        if (IsProvider != isProvider)
        {
            IsProvider = isProvider;
            NotifyStateChanged();
        }
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_userToken);

    public event Action? OnChange;

    public void TriggerStateChanged() => OnChange?.Invoke();

    private void NotifyStateChanged() => OnChange?.Invoke();
}
