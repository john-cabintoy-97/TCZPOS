using BarcodeScanning;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using CurrieTechnologies.Razor.Vibration;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Fonts;
using TCZPOS.Components.Database;
using TCZPOS.Components.Extension;
using TCZPOS.Components.Providers;
using TCZPOS.Components.ServiceRegistration;
using TCZPOS.Components.Services.Hardware;

namespace TCZPOS
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            SQLitePCL.Batteries_V2.Init();
            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitCore()
                .UseMauiCommunityToolkitCamera()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                })
                .UseBarcodeScanning();
            GlobalFontSettings.FontResolver = new MyFontResolver();
            builder.Services.AddSingleton<DBQueries>();
            builder.Services.AddSingleton<ConnectionTemplate>();
            builder.Services.AddVibration();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddPOSServices();
            builder.Services.AddSingleton<AuthStateProvider>();
            builder.Services.AddScoped<AlertServices>();
            builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<AuthStateProvider>());
            builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            //builder.Services.AddScoped(sp =>
            //{
            //    return new HttpClient
            //    {
            //        // This is the base URL for your PC's API
            //        BaseAddress = new Uri("http://192.168.1.35:5039/")
            //    };
            //});

            return builder.Build();
        }

    }
}
