using CommunityToolkit.Mvvm.Messaging;
using TCZPOS.Xaml;
using static TCZPOS.Xaml.CameraNativePage;

namespace TCZPOS.Components.Services.Hardware
{
    public class ScannerNativeServices(HardwareServices hardwareServices)
    {
        private bool _isClosing = false;
        public async Task<string?> CapturePhotoAsync()
        {
            _isClosing = false;

            // 1. Request Permission
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted) return null;

            // 2. Clean up any previous subscriptions
            WeakReferenceMessenger.Default.Unregister<CameraNativePage.OCRResultUpdateMessage>(this);

            var tcs = new TaskCompletionSource<string?>();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var mainPage = Application.Current?.Windows[0]?.Page;
                if (mainPage?.Navigation == null)
                {
                    tcs.TrySetResult(null);
                    return;
                }

                try
                {
                    var photoPage = new CameraNativePage();

                    // 3. Listen for the NATIVE OCR Result
                    WeakReferenceMessenger.Default.Register<CameraNativePage.OCRResultUpdateMessage>(this, async (r, m) =>
                    {
                        if (_isClosing) return;
                        _isClosing = true;

                        WeakReferenceMessenger.Default.Unregister<CameraNativePage.OCRResultUpdateMessage>(this);

                        await hardwareServices.VibrateFeedback();

                        // On Android, m.DetectedText contains the actual product name/info
                        tcs.TrySetResult(m.DetectedText);
                    });

                    // 4. Handle Cancellation (Back button or Close)
                    photoPage.Disappearing += (s, e) =>
                    {
                        if (!_isClosing)
                        {
                            _isClosing = true;
                            WeakReferenceMessenger.Default.Unregister<CameraNativePage.OCRResultUpdateMessage>(this);
                            tcs.TrySetResult(null);
                        }
                    };

                    await mainPage.Navigation.PushModalAsync(photoPage);
                }
                catch (Exception ex)
                {
                    _isClosing = false;
                    WeakReferenceMessenger.Default.Unregister<CameraNativePage.OCRResultUpdateMessage>(this);
                    tcs.TrySetException(ex);
                }
            });

            return await tcs.Task;
        }
        public async Task<string?> ReadBarcodeAsync(string mode = "BARCODE", bool showManual = true)
        {
            _isClosing = false; 

            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                return null;
            }

            var tcs = new TaskCompletionSource<string?>();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var app = Application.Current;
                if (app?.Windows is not { Count: > 0 } windows)
                {
                    tcs.TrySetResult(null);
                    return;
                }
                var mainPage = windows[0].Page;

                if (mainPage?.Navigation == null)
                {
                    tcs.TrySetResult(null);
                    return;
                }

                try
                {
                    var scannerPage = new ScannerPage(hardwareServices)
                    {
                        ScanMode = mode,
                        IsManualInputVisible = showManual 
                    };

                    // Handle scan result
                    scannerPage.OnScanResult += async (result) =>
                    {
                        if (_isClosing) return;
                        _isClosing = true;
                        bool isRealScan = !string.IsNullOrEmpty(result) && result != "[MANUAL_INPUT]";

                        if (isRealScan)
                        {
                            // 2. Check the mode
                            if (mode.Equals("PAIRING", StringComparison.OrdinalIgnoreCase))
                            {
                                await hardwareServices.VibrateFeedback();
                            }
                            else
                            {
                                // Full feedback (Beep + Vibrate) for Inventory/Products
                                await hardwareServices.ProvideScanFeedback();
                            }
                        }
                        scannerPage.StopScanner();

                        // Use the mainPage variable we captured earlier
                        if (mainPage.Navigation.ModalStack.Count > 0)
                        {
                            await mainPage.Navigation.PopModalAsync(true);
                        }

                        await Task.Delay(300);
                        tcs.TrySetResult(result);
                    };

                    scannerPage.Unfocused += (s, e) => {
                        if (!_isClosing) tcs.TrySetResult(null);
                    };
                    await mainPage.Navigation.PushModalAsync(scannerPage);
                }
                catch (Exception ex)
                {
                    _isClosing = false;
                    tcs.TrySetException(ex);
                }
            });
            return await tcs.Task;
        }
    }
}