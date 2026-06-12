namespace TCZPOS
{
    public partial class App : Application
    {
        private readonly MainPage _mainPage;

        public App(MainPage mainPage)
        {
            InitializeComponent();
           // SecureStorage.Default.Remove("IsLoggedIn");
            _mainPage = mainPage;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(_mainPage) { Title = "TCZPOS" };
//#if WINDOWS
//            window.Created += (s, e) =>
//            {
//                // Get the native Windows handle
//                var handle = WinRT.Interop.WindowNative.GetWindowHandle(window.Handler.PlatformView);
//                var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
//                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);

//                // Option 1: True Full Screen (Hides Taskbar - Best for Kiosks)
//                appWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);

//                /* // Option 2: Just Maximized (Taskbar remains visible)
//                var presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
//                if (presenter != null)
//                {
//                    presenter.Maximize();
//                }
//                */
//            };
//#endif
            return window;
        }
    }
}
