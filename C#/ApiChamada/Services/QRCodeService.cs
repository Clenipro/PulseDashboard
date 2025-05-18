namespace ApiChamada.Services
{
    public class QRCodeService
    {
        public bool VerificarOrigemQRCode(string qrCodeData)
        {
            // Remove possíveis espaços em branco e verifica
            qrCodeData = qrCodeData?.Trim();

            // Verifica se não está vazio e tem um formato de QR Code
            return !string.IsNullOrEmpty(qrCodeData) &&
                   (
                       // Alguns padrões comuns de QR Codes
                       qrCodeData.StartsWith("QR_") ||
                       qrCodeData.StartsWith("TICKET_") ||
                       qrCodeData.Contains("TICKET") ||
                       // Verifica se tem um comprimento típico de QR Code
                       (qrCodeData.Length >= 10 && qrCodeData.Length <= 50) ||
                       // Pode adicionar mais validações específicas do seu sistema
                       qrCodeData.All(char.IsLetterOrDigit)
                   );
        }
    }
}