using QRCoder;

namespace TCZPOS.Components.Services.Hardware
{
    public class QRCodeServices()
    {
        public static string GenerateBase64QR(string payload, int pixelsPerModule = 20)
        {
            if (string.IsNullOrEmpty(payload)) return string.Empty;

            using QRCodeGenerator qrGenerator = new();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using PngByteQRCode qrCode = new(qrCodeData);

            byte[] qrImageBytes = qrCode.GetGraphic(pixelsPerModule);
            return $"data:image/png;base64,{Convert.ToBase64String(qrImageBytes)}";
        }
    }
}