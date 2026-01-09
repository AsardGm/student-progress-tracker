using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

/// <summary>
/// Rozhraní pro službu spravující skupiny a jejich členy.
/// Poskytuje CRUD operace pro skupiny, správu členů a generování QR kódů.
/// </summary>
public interface IGroupService
{
    /// <summary>
    /// Vytvoří novou skupinu s definovanými cíli.
    /// Automaticky vygeneruje přístupový kód a QR kód.
    /// </summary>
    /// <param name="name">Název skupiny.</param>
    /// <param name="description">Popis skupiny/úkolu.</param>
    /// <param name="goals">Seznam cílů pro skupinu.</param>
    /// <param name="ownerId">ID učitele vlastnícího skupinu.</param>
    /// <returns>Vytvořená skupina.</returns>
    Task<Group> CreateGroupAsync(string name, string description, List<GoalDto> goals, string ownerId);

    /// <summary>
    /// Získá skupinu podle ID včetně všech souvisejících dat.
    /// </summary>
    /// <param name="groupId">ID skupiny.</param>
    /// <returns>Skupina nebo null pokud neexistuje.</returns>
    Task<Group?> GetGroupAsync(Guid groupId);

    /// <summary>
    /// Získá skupinu podle přístupového kódu.
    /// Používá se při připojování studentů.
    /// </summary>
    /// <param name="joinCode">6-znakový přístupový kód.</param>
    /// <returns>Aktivní skupina nebo null.</returns>
    Task<Group?> GetGroupByJoinCodeAsync(string joinCode);

    /// <summary>
    /// Získá všechny skupiny vlastněné daným učitelem.
    /// </summary>
    /// <param name="ownerId">ID učitele.</param>
    /// <returns>Seznam skupin seřazených od nejnovější.</returns>
    Task<List<Group>> GetGroupsByOwnerAsync(string ownerId);

    /// <summary>
    /// Přidá studenta do skupiny.
    /// Automaticky inicializuje pokrok pro všechny cíle.
    /// </summary>
    /// <param name="groupId">ID skupiny.</param>
    /// <param name="deviceId">Unikátní ID zařízení studenta.</param>
    /// <param name="nickname">Přezdívka studenta.</param>
    /// <returns>Vytvořený záznam člena.</returns>
    Task<GroupMember> JoinGroupAsync(Guid groupId, string deviceId, string nickname);

    /// <summary>
    /// Získá člena skupiny podle ID zařízení.
    /// Používá se pro rozpoznání vracejících se studentů.
    /// </summary>
    Task<GroupMember?> GetMemberByDeviceIdAsync(Guid groupId, string deviceId);

    /// <summary>
    /// Získá všechny členy skupiny s jejich pokroky.
    /// </summary>
    Task<List<GroupMember>> GetGroupMembersAsync(Guid groupId);

    /// <summary>
    /// Zkontroluje, zda zařízení již je členem skupiny.
    /// </summary>
    Task<bool> HasDeviceJoinedGroupAsync(Guid groupId, string deviceId);

    /// <summary>
    /// Aktualizuje skupinu v databázi.
    /// </summary>
    Task UpdateGroupAsync(Group group);

    /// <summary>
    /// Smaže skupinu a všechna související data.
    /// </summary>
    Task DeleteGroupAsync(Guid groupId);

    /// <summary>
    /// Získá uživatele podle e-mailu.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email);
}

/// <summary>
/// Data transfer object pro vytváření cílů.
/// Používá se při vytváření skupiny.
/// </summary>
public class GoalDto
{
    /// <summary>Název cíle.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Popis cíle pro AI analýzu.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Typ cíle (Boolean/Percentage).</summary>
    public GoalType Type { get; set; }

    /// <summary>Cílová hodnota pro procentuální cíle.</summary>
    public int TargetValue { get; set; } = 1;
}
