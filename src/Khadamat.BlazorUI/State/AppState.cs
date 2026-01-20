using System;

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

    public string UserName { get; set; } = "أحمد محمد";
    public string UserRole { get; set; } = "مستخدم";
    public string UserImageUrl { get; set; } = "https://i.pravatar.cc/150?u=antigravity";
    public bool IsProvider { get; set; }
    
    // UI State
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

    private void NotifyStateChanged() => OnChange?.Invoke();
}
