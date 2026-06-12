using Microsoft.Maui.Controls;
using TCZPOS.Components.Services.Hardware;

namespace TCZPOS
{
    public partial class MainPage : ContentPage
    {
        private PairingListenerServices _listener = new();

        public MainPage(PairingListenerServices listener)
        {
            InitializeComponent();
            _listener = listener;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Start the listener in the background when the page is shown
            Task.Run(async () =>
            {
                try
                {
                    await _listener.StartListening();
                }
                catch (Exception ex)
                {
                    // This will catch errors if the port is already in use
                    Console.WriteLine($"[QPOS ERROR] Listener failed: {ex.Message}");
                }
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Clean up the listener when the page is closed
            _listener.Stop();
        }
    }
}