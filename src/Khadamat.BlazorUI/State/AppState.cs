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

    private string _userName = string.Empty;
    public string UserName 
    { 
        get => _userName; 
        set { if (_userName != value) { _userName = value; NotifyStateChanged(); } } 
    }

    private string _userRole = string.Empty;
    public string UserRole 
    { 
        get => _userRole; 
        set { if (_userRole != value) { _userRole = value; NotifyStateChanged(); } } 
    }

    private string _userId = string.Empty;
    public string UserId 
    { 
        get => _userId; 
        set { if (_userId != value) { _userId = value; NotifyStateChanged(); } } 
    }
    
    private string _userImageUrl = string.Empty;
    public string UserImageUrl 
    { 
        get => string.IsNullOrEmpty(_userImageUrl) ? DefaultImages.DefaultAvatar : _userImageUrl;
        set { if (_userImageUrl != value) { _userImageUrl = value; NotifyStateChanged(); } }
    }

    private bool _isProvider;
    public bool IsProvider 
    { 
        get => _isProvider; 
        set { if (_isProvider != value) { _isProvider = value; NotifyStateChanged(); } } 
    }

    private bool _isProviderMode;
    public bool IsProviderMode 
    { 
        get => _isProviderMode; 
        set { if (_isProviderMode != value) { _isProviderMode = value; NotifyStateChanged(); } } 
    }
    
    public int NotificationCount { get; set; } = 3;
    public bool HasUnreadNotifications { get; set; }
    public bool HasUnreadMessages { get; set; }

    // Global Settings
    public string AppName { get; set; } = "خدمات";
    public string AppLogo { get; set; } = "";
    public string PrimaryColor { get; set; } = "#6366f1";
    public string SecondaryColor { get; set; } = "#a855f7";
   // UI State
    public bool IsSidebarOpen { get; set; }
    private bool _isMobileApp;
    public bool IsMobileApp 
    { 
        get => _isMobileApp; 
        set { if (_isMobileApp != value) { _isMobileApp = value; NotifyStateChanged(); } } 
    }
    
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

    public void UpdateUserStatus(string userName, string userRole, bool isProvider, string imageUrl, string userId = "")
    {
        UserName = userName;
        UserRole = userRole;
        IsProvider = isProvider;
        UserImageUrl = imageUrl;
        UserId = userId;
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
