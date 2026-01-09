using QRCoder;

namespace StudentProgressTracker.Services;

/// <summary>
/// Implementace služby pro generování QR kódů.
/// Využívá knihovnu QRCoder pro vytváření PNG obrázků QR kódů.
/// </summary>
public class QRCodeService : IQRCodeService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Inicializuje novou instanci QRCodeService.
    /// </summary>
    /// <param name="configuration">Konfigurace aplikace pro získání BaseUrl.</param>
    public QRCodeService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public string GenerateQRCode(string content)
    {
        // Získání base URL z konfigurace (fallback na localhost pro vývoj)
        var baseUrl = (_configuration["AppSettings:BaseUrl"] ?? "https://localhost:7001").TrimEnd('/');
        var fullUrl = $"{baseUrl}{content}";
        return GenerateQRCodeFromUrl(fullUrl);
    }

    /// <inheritdoc />
    public string GenerateQRCodeUrl(string baseUrl, string joinCode)
    {
        var fullUrl = $"{baseUrl}/join/{joinCode}";
        return GenerateQRCodeFromUrl(fullUrl);
    }

    /// <summary>
    /// Generuje QR kód z URL a vrací ho jako Base64 data URI.
    /// </summary>
    /// <param name="url">Kompletní URL pro zakódování.</param>
    /// <returns>Data URI ve formátu "data:image/png;base64,...".</returns>
    private static string GenerateQRCodeFromUrl(string url)
    {
        // Vytvoření QR kódu s úrovní korekce chyb Q (25%)
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        // Generování PNG s rozlišením 10 pixelů na modul
        var qrCodeBytes = qrCode.GetGraphic(10);

        // Vrácení jako data URI pro přímé použití v HTML img tagu
        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }
}
