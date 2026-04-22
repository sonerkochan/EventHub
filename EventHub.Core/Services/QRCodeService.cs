using System;
using System.Collections.Generic;
using System.Text;
using QRCoder;

namespace EventHub.Core.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCode(string payload, int size = 10);
    }

    public class QRCodeService : IQRCodeService
    {
        public string GenerateQRCode(string payload, int size = 10)
        {
            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Payload cannot be null or empty", nameof(payload));

            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] pngBytes = qrCode.GetGraphic(size);

            return Convert.ToBase64String(pngBytes);
        }
    }
}
