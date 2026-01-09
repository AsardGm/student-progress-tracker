using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

/// <summary>
/// Rozhraní pro AI službu poskytující analýzu zpráv a generování odpovědí.
/// Implementace používá Google Gemini API.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Analyzuje zprávu studenta v kontextu jeho cílů.
    /// Vrací strukturovanou analýzu s hodnocením relevance, pokroku a případné potřeby pomoci.
    /// </summary>
    /// <param name="message">Text zprávy studenta.</param>
    /// <param name="goals">Seznam cílů skupiny.</param>
    /// <param name="recentMessages">Posledních N zpráv pro kontext konverzace.</param>
    /// <returns>Strukturovaná AI analýza zprávy.</returns>
    Task<AIAnalysis> AnalyzeMessageAsync(string message, List<Goal> goals, List<ChatMessage> recentMessages);

    /// <summary>
    /// Generuje odpověď AI asistenta na zprávu studenta.
    /// Bere v úvahu aktuální pokrok studenta a kontext konverzace.
    /// </summary>
    /// <param name="studentMessage">Text zprávy studenta.</param>
    /// <param name="goals">Seznam cílů skupiny.</param>
    /// <param name="progress">Aktuální pokrok studenta u jednotlivých cílů.</param>
    /// <param name="recentMessages">Posledních N zpráv pro kontext.</param>
    /// <returns>Text odpovědi pro studenta v češtině.</returns>
    Task<string> GenerateResponseAsync(string studentMessage, List<Goal> goals, List<MemberProgress> progress, List<ChatMessage> recentMessages);

    /// <summary>
    /// Generuje uvítací zprávu pro studenta při připojení do skupiny.
    /// Obsahuje přehled cílů a povzbuzení k začátku práce.
    /// </summary>
    /// <param name="group">Skupina s definovanými cíli.</param>
    /// <returns>Uvítací zpráva v češtině.</returns>
    Task<string> GenerateWelcomeMessageAsync(Group group);

    /// <summary>
    /// Generuje nápovědu pro konkrétní cíl na základě aktuálního pokroku.
    /// </summary>
    /// <param name="goal">Cíl, ke kterému se nápověda vztahuje.</param>
    /// <param name="progress">Aktuální pokrok studenta u cíle.</param>
    /// <returns>Text nápovědy v češtině.</returns>
    Task<string> GenerateHintAsync(Goal goal, MemberProgress progress);
}
