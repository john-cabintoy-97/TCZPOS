using Microsoft.Maui.Storage;

namespace TCZPOS.Components.Extension
{
    public class FileLauncherExt
    {
        public async Task OpenFileAsync(string filePath, string mimeType)
        {
            try
            {
#if ANDROID
                // 1. Create Java File object
                var javaFile = new Java.IO.File(filePath);

                // 2. Get URI via FileProvider
                // Note: Ensure this matches your AndroidManifest.xml exactly
                var contentUri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                    Platform.CurrentActivity,
                    "com.companyname.tczpos.fileprovider",
                    javaFile);

                // 3. Setup Intent
                var intent = new Android.Content.Intent(Android.Content.Intent.ActionView);
                intent.SetDataAndType(contentUri, mimeType);
                intent.SetFlags(Android.Content.ActivityFlags.GrantReadUriPermission | Android.Content.ActivityFlags.NewTask);

                // 4. Launch
                Platform.CurrentActivity.StartActivity(intent);
#else
                // Windows/iOS default launcher
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
#endif
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to open file: {ex.Message}");
            }
        }
    }
}
