using Android.App;
using Android.Runtime;

namespace Khadamat.MobileApp;

[Register("com.nassar84.khadamat.MainApplication")]
[Application(Name = "com.nassar84.khadamat.MainApplication")]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
