using TCZPOS.Components.Extension;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;

#if ANDROID
using Android.Graphics;
using Xamarin.Google.MLKit.Vision.Text;
using Xamarin.Google.MLKit.Vision.Common;
using Xamarin.Google.MLKit.Vision.Text.Latin;
#endif

namespace TCZPOS.Xaml;

public partial class CameraNativePage : ContentPage
{
    private bool _isProcessing = false;

    public CameraNativePage()
    {
        InitializeComponent();

        this.Appearing += async (s, e) =>
        {
            await Task.Delay(400);
            await cameraView.FadeToAsync(1, 250);
        };
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        _isProcessing = true;

        try
        {
            await Task.Delay(100);
            cameraView.Handler?.DisconnectHandler();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cleanup Error: {ex.Message}");
        }
    }

    private void OnFlashClicked(object sender, EventArgs e)
    {
        if (cameraView.CameraFlashMode == CameraFlashMode.Off)
        {
            cameraView.CameraFlashMode = CameraFlashMode.On;
            btnFlash.Text = IconFont.FlashOn;
            btnFlash.BackgroundColor = Microsoft.Maui.Graphics.Color.FromRgba("#FFD700");
            btnFlash.TextColor = Colors.Black;
        }
        else
        {
            cameraView.CameraFlashMode = CameraFlashMode.Off;
            btnFlash.Text = IconFont.FlashOff;
            btnFlash.BackgroundColor = Microsoft.Maui.Graphics.Color.FromRgba("#33FFFFFF");
            btnFlash.TextColor = Colors.White;
        }
    }

    private async void OnBrowseClicked(object sender, EventArgs e)
    {
        if (_isProcessing) return;
        try
        {
            var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = "Select Product Photo" });
            var photo = results?.FirstOrDefault();
            if (photo != null)
            {
                using var stream = await photo.OpenReadAsync();
                await ProcessAndSendStream(stream);
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Browse Error: {ex.Message}"); }
    }

    private async void OnCaptureClicked(object sender, EventArgs e)
    {
        if (_isProcessing) return;
       
        try
        {
            _isProcessing = true;
            _ = shutterOverlay.FadeToAsync(0.8, 50).ContinueWith(async t =>
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await shutterOverlay.FadeToAsync(0, 100);
                });
            });
            var stream = await cameraView.CaptureImage(CancellationToken.None);
            if (stream != null) await ProcessAndSendStream(stream);
            else _isProcessing = false;
        }
        catch (Exception ex)
        {
            _isProcessing = false;
            System.Diagnostics.Debug.WriteLine($"Capture Error: {ex.Message}");
        }
    }

    private async Task ProcessAndSendStream(Stream stream)
    {
        _isProcessing = true;
        loadingIndicator.IsVisible = true;
        loadingIndicator.IsRunning = true;

        try
        {
            string detectedText = string.Empty;

#if ANDROID
            detectedText = await Task.Run(async () =>
            {
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                Bitmap? bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                InputImage visionImage = InputImage.FromBitmap(bitmap!, 0);

                var recognizer = TextRecognition.GetClient(TextRecognizerOptions.DefaultOptions);

                // Bridging Java Tasks to C# Tasks
                var tcs = new TaskCompletionSource<string>();

                recognizer.Process(visionImage)
                    .AddOnSuccessListener(new OnSuccessListener((result) => {
                        if (result is Xamarin.Google.MLKit.Vision.Text.Text textResult)
                        {
                            tcs.TrySetResult(textResult.GetText() ?? string.Empty);
                        }
                        else
                        {
                            tcs.TrySetResult(string.Empty);
                        }
                    }));

                return await tcs.Task;
            });
#else
            detectedText = "OCR only available on Android hardware.";
#endif

            WeakReferenceMessenger.Default.Send(new OCRResultUpdateMessage(detectedText));

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Navigation.ModalStack.Count > 0)
                    await Navigation.PopModalAsync();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Native OCR Error: {ex.Message}");
        }
        finally
        {
            loadingIndicator.IsRunning = false;
            loadingIndicator.IsVisible = false;
            _isProcessing = false;
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        if (_isProcessing) return;
        _isProcessing = true;
        if (Navigation.ModalStack.Count > 0) await Navigation.PopModalAsync();
    }

    public record OCRResultUpdateMessage(string DetectedText);
}

#if ANDROID
public class OnSuccessListener : Java.Lang.Object, Android.Gms.Tasks.IOnSuccessListener
{
    // Fix: Added '?' to allow the Java object to be null
    private readonly Action<Java.Lang.Object?> _callback;

    public OnSuccessListener(Action<Java.Lang.Object?> callback) => _callback = callback;

    public void OnSuccess(Java.Lang.Object? result) => _callback?.Invoke(result);
}

public class OnFailureListener : Java.Lang.Object, Android.Gms.Tasks.IOnFailureListener
{
    private readonly Action<Java.Lang.Exception> _callback;
    public OnFailureListener(Action<Java.Lang.Exception> callback) => _callback = callback;
    public void OnFailure(Java.Lang.Exception e) => _callback?.Invoke(e);
}
#endif