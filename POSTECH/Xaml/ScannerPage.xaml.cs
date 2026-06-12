namespace TCZPOS.Xaml;
using BarcodeScanning;
using TCZPOS.Components.Extension;
using TCZPOS.Components.Services.Hardware;
using System.Linq;

public partial class ScannerPage : ContentPage
{
    public event Action<string>? OnScanResult;
    private bool _stopAnimation = false;
    private string _currentCode = "";
    private bool _isDismissing = false;
    public bool IsContinuousMode { get; set; } = false;
    public string ScanMode { get; set; } = "BARCODE";
    public bool IsManualInputVisible { get; set; } = true;
    private readonly HardwareServices _hardwareServices;

    public ScannerPage(HardwareServices hardwareServices)
    {
        InitializeComponent();
        _hardwareServices = hardwareServices;
        //barcodeReader.VerticalOptions = new BarcodeReaderOptions
        //{
        //    Formats = BarcodeFormat.Code128,
        //    AutoRotate = true,
        //    Multiple = false,
        //    TryInverted = true
        //};

    }

    private void ToggleTorch(object? sender, EventArgs e)
    {
        barcodeReader.TorchOn = !barcodeReader.TorchOn;
        TorchBtn.Text = barcodeReader.TorchOn
            ? IconFont.FlashOff
            : IconFont.FlashOn;

        TorchBtn.FontFamily = "MaterialIcons";

        TorchBtn.BackgroundColor = barcodeReader.TorchOn
            ? Color.FromArgb("#FFD700")
            : Color.FromArgb("#40FFFFFF");

        TorchBtn.TextColor = barcodeReader.TorchOn ? Colors.Black : Colors.White;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateUIForMode();
        RefreshContinuousModeUI();
        barcodeReader.CameraEnabled = true;
        await Methods.AskForRequiredPermissionAsync();
        _stopAnimation = false;
        AnimateLaser();
    }

    private void AnimateLaser()
    {
        if (_isDismissing) return;
        var laserAnimation = new Animation(v => LaserLine.TranslationY = v, 0, 280);
        laserAnimation.Commit(this, "ScannerLaser", 16, 2000, Easing.Linear,
            finished: (v, c) => LaserLine.TranslationY = 0,
            repeat: () => !_stopAnimation);
    }

    private void UpdateUIForMode()
    {
        var mode = ScanMode?.ToUpper() ?? "BARCODE";

        switch (mode)
        {
            case "PAIRING":
                InstructionLabel.Text = "SCAN TERMINAL QR CODE";
                ManualInputBtn.IsVisible = false;
                LaserLine.Color = Color.FromArgb("#FFD700");
                break;
            case "INVENTORY":
                InstructionLabel.Text = "INVENTORY MODE: SCAN ITEMS";
                ManualInputBtn.IsVisible = true;
                LaserLine.Color = Color.FromArgb("#00FF00");
                break;
            default:
                InstructionLabel.Text = "ALIGN BARCODE TO SCAN";
                ManualInputBtn.IsVisible = false;
                LaserLine.Color = Color.FromArgb("#FF0044");
                break;
        }
    }

    private void BarcodesDetected(object sender, OnDetectionFinishedEventArg e)
    {
        // The native scanner runs on a background thread, so always use MainThread
        MainThread.BeginInvokeOnMainThread(async () => {
            // Access results via e.BarcodeResults
            var result = e.BarcodeResults?.FirstOrDefault();

            if (result != null && !BottomSheet.IsVisible && !_isDismissing)
            {
                _currentCode = result.DisplayValue; // Use DisplayValue instead of Value

                if (_hardwareServices.IsContinuousModeActive)
                {
                    await _hardwareServices.VibrateFeedback();

                    // Pause detection manually if needed
                    barcodeReader.PauseScanning = true ;

                    OnScanResult?.Invoke(_currentCode);

                    await Task.Delay(1500);
                    barcodeReader.PauseScanning = false;
                }
                else
                {
                    barcodeReader.PauseScanning = true;
                    _stopAnimation = true;
                    await _hardwareServices.VibrateFeedback();
                    await ShowBottomSheet(_currentCode);
                }
            }
        });
    }

    private void ToggleContinuousMode(object? sender, EventArgs e)
    {
        _hardwareServices.IsContinuousModeActive = !_hardwareServices.IsContinuousModeActive;
        RefreshContinuousModeUI();
    }
    private async void OnManualInputClicked(object? sender, EventArgs e)
    {
        await SafeCloseScanner(() => OnScanResult?.Invoke("[MANUAL_INPUT]"));
    }

    private async Task ShowBottomSheet(string code)
    {
        var mode = ScanMode?.ToUpper() ?? "BARCODE";
        BarcodeResultLabel.MaxLines = 1;
        BarcodeResultLabel.LineBreakMode = LineBreakMode.TailTruncation;
        switch (mode)
        {
            case "PAIRING":
                PopupHeaderLabel.Text = "PAIRING REQUEST";
                BarcodeResultLabel.Text = "Connect to Terminal?";
                ConfirmBtn.Text = "PAIR NOW";
                BarcodeResultLabel.MaxLines = 2;
                BarcodeResultLabel.LineBreakMode = LineBreakMode.WordWrap;
                ConfirmBtn.BackgroundColor = Color.FromArgb("#FFD700");
                break;

            case "INVENTORY":
                PopupHeaderLabel.Text = "IDENTIFIED";
                BarcodeResultLabel.Text = $"SKU: {code}";
                ConfirmBtn.Text = "ADD BARCODE";
                ConfirmBtn.BackgroundColor = Color.FromArgb("#00FF00");
                break;

            default:
                PopupHeaderLabel.Text = "BARCODE IDENTIFIED";
                BarcodeResultLabel.Text = code;
                ConfirmBtn.Text = "ADD TO CART";
                ConfirmBtn.BackgroundColor = Color.FromArgb("#FFD700");
                break;
        }

        HelperLayout.IsVisible = false;
        BottomSheet.IsVisible = true;

        // Animate the sheet sliding up from the bottom
        await BottomSheet.TranslateToAsync(0, 0, 300, Easing.SpringOut);
    }

    private async void ClosePopup(object? sender, EventArgs e)
    {
        await BottomSheet.TranslateToAsync(0, 350, 250, Easing.CubicIn);
        BottomSheet.IsVisible = false;
        HelperLayout.IsVisible = true;

        _stopAnimation = false;
        barcodeReader.PauseScanning = false;
        AnimateLaser();
    }

    private async void HandleConfirmation(object? sender, EventArgs e)
    {
        await SafeCloseScanner(() => OnScanResult?.Invoke(_currentCode));
    }

    public void StopScanner()
    {
        barcodeReader.CameraEnabled = false;
    }

    private async Task SafeCloseScanner(Action? onResult = null)
    {
        if (_isDismissing) return;
        _isDismissing = true;

        barcodeReader.TorchOn = false;
        barcodeReader.CameraEnabled = false;
        _stopAnimation = true;

        await Task.Delay(50);

        onResult?.Invoke();

        // 4. Pop the UI on the Main Thread
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
            }
        });
    }

    private async void CloseClicked(object? sender, EventArgs e)
    {
        await SafeCloseScanner(() => OnScanResult?.Invoke(null!));
    }

    private void RefreshContinuousModeUI()
    {
        bool isActive = _hardwareServices.IsContinuousModeActive;

        ContinuousBtn.Text = IconFont.Repeat;
        ContinuousBtn.FontFamily = "MaterialIcons";
        ContinuousBtn.BackgroundColor = isActive ? Color.FromArgb("#FFD700") : Color.FromArgb("#40000000");
        ContinuousBtn.TextColor = isActive ? Colors.Black : Colors.White;

        if (isActive)
        {
            InstructionLabel.Text = "AUTO-SCANNING MODE";
            InstructionLabel.TextColor = Color.FromArgb("#FFD700");
        }
        else
        {
            InstructionLabel.TextColor = Colors.White;
            UpdateUIForMode();
        }
    }

    
}