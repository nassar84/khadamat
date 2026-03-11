using Khadamat.MobileApp.Views;

namespace Khadamat.MobileApp.Views
{
    public class HomePage : WebContainerPage { public HomePage() : base("") { } }
    public class CategoriesPage : WebContainerPage { public CategoriesPage() : base("explore") { } }
    public class MyServicesPage : WebContainerPage { public MyServicesPage() : base("provider/dashboard") { } }
    public class FavoritesPage : WebContainerPage { public FavoritesPage() : base("favorites") { } }
    public class SettingsPage : WebContainerPage { public SettingsPage() : base("settings") { } }
    public class ServicesPage : WebContainerPage { public ServicesPage() : base("services") { } }
    public class PostServicePage : WebContainerPage { public PostServicePage() : base("provider/apply") { } }
    public class MessagesPage : WebContainerPage { public MessagesPage() : base("messages") { } }
    public class ProfilePage : WebContainerPage { public ProfilePage() : base("profile") { } }
    public class MarketplacePage : WebContainerPage { public MarketplacePage() : base("marketplace") { } }
}
