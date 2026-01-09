namespace StudentProgressTracker.Services;

/// <summary>
/// Rozhraní pro službu generující QR kódy.
/// Používá se pro vytváření QR kódů s URL pro připojení studentů do skupin.
/// </summary>
public interface IQRCodeService
{
    /// <summary>
    /// Generuje QR kód pro daný obsah.
    /// Automaticky přidá base URL z konfigurace.
    /// </summary>
    /// <param name="content">Relativní cesta (např. "/join/ABC123").</param>
    /// <returns>QR kód jako Base64 PNG data URI.</returns>
    string GenerateQRCode(string content);

    /// <summary>
    /// Generuje QR kód pro konkrétní URL a přístupový kód.
    /// </summary>
    /// <param name="baseUrl">Základní URL aplikace.</param>
    /// <param name="joinCode">Přístupový kód skupiny.</param>
    /// <returns>QR kód jako Base64 PNG data URI.</returns>
    string GenerateQRCodeUrl(string baseUrl, string joinCode);
}
