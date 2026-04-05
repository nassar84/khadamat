using Android.App;
using Android.Runtime;

namespace Khadamat.MobileApp;

[Register("com.khadamat.app.MainApplication")]
[Application(Name = "com.khadamat.app.MainApplication")]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
