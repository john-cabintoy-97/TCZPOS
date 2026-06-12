using Android.App;
using Android.Runtime;

namespace TCZPOS
{
    [Application(UsesCleartextTraffic = true)] // <--- ADD THIS LINE
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
