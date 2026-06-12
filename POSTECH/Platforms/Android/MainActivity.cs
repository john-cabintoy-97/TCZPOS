using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Webkit;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.ApplicationModel; // For Platform.Init
using Microsoft.Maui.Handlers;

namespace TCZPOS
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("MyCustomBlazorWebViewMapper", (handler, view) =>
            {
                handler.PlatformView.SetWebChromeClient(new MyWebChromeClient());
            });
        }

        // Add this to handle the result of the permission pop-up
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    // This helper class tells the WebView to grant the JS request for camera/mic
    public class MyWebChromeClient : WebChromeClient
    {
        // Added '?' to PermissionRequest to match the base class nullability
        public override void OnPermissionRequest(PermissionRequest? request)
        {
            // Use the null-conditional operator '?.' just to be safe
            request?.Grant(request.GetResources());
        }
    }
}