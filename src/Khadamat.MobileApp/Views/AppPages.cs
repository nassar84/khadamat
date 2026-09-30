using Khadamat.MobileApp.Views;

namespace Khadamat.MobileApp.Views
{
    public class HomePage : WebContainerPage { public HomePage() : base("") { } }
    public class CategoriesPage : WebContainerPage { public CategoriesPage() : base("categories") { } }
    public class MyServicesPage : WebContainerPage { public MyServicesPage() : base("provider/my-services") { } }
    public class FavoritesPage : WebContainerPage { public FavoritesPage() : base("client/favorites") { } }
    public class SettingsPage : WebContainerPage { public SettingsPage() : base("settings") { } }
    public class ServicesPage : WebContainerPage { public ServicesPage() : base("services") { } }
    public class PostServicePage : WebContainerPage { public PostServicePage() : base("provider/apply") { } }
    public class MessagesPage : WebContainerPage { public MessagesPage() : base("messages") { } }
    public class ProfilePage : WebContainerPage { public ProfilePage() : base("profile") { } }
    public class MarketplacePage : WebContainerPage { public MarketplacePage() : base("marketplace") { } }
    public class LoginPage : WebContainerPage { public LoginPage() : base("login") { } }
    public class TermsPage : WebContainerPage { public TermsPage() : base("terms") { } }
    public class AdminPage : WebContainerPage { public AdminPage() : base("admin") { } }
    public class AdminAdsPage : WebContainerPage { public AdminAdsPage() : base("admin/ads") { } }
    public class SupportPage : WebContainerPage { public SupportPage() : base("contact") { } }
    public class FeedPage : WebContainerPage { public FeedPage() : base("feed") { } }
    public class MyRequestsPage : WebContainerPage { public MyRequestsPage() : base("user/my-requests") { } }
    public class ProviderDashboardPage : WebContainerPage { public ProviderDashboardPage() : base("provider/dashboard") { } }
    public class CreateServicePage : WebContainerPage { public CreateServicePage() : base("provider/create-request") { } }
    public class ApprovalsPage : WebContainerPage { public ApprovalsPage() : base("admin/approvals") { } }
    public class ComplaintsPage : WebContainerPage { public ComplaintsPage() : base("admin/complaints") { } }
}
