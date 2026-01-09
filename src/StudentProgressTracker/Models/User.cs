namespace StudentProgressTracker.Models;

/// <summary>
/// Reprezentuje uživatele systému (učitel nebo administrátor).
/// Uživatelé se autentizují pomocí Google OAuth.
/// </summary>
public class User
{
    /// <summary>
    /// Unikátní identifikátor uživatele (GUID).
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// E-mailová adresa uživatele z Google účtu.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Unikátní identifikátor z Google OAuth (sub claim).
    /// </summary>
    public string? GoogleId { get; set; }

    /// <summary>
    /// Zobrazované jméno uživatele z Google profilu.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Role uživatele v systému (Student, Teacher, Admin).
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Student;

    /// <summary>
    /// Datum a čas vytvoření účtu (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Skupiny, které uživatel vlastní (je jejich učitelem).
    /// </summary>
    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();

    /// <summary>
    /// Žádosti o pomoc, které uživatel vyřešil jako učitel.
    /// </summary>
    public ICollection<HelpRequest> ResolvedHelpRequests { get; set; } = new List<HelpRequest>();
}

/// <summary>
/// Definuje možné role uživatelů v systému.
/// Role určuje přístupová práva (RBAC).
/// </summary>
public enum UserRole
{
    /// <summary>Student - základní role, nemůže vytvářet skupiny.</summary>
    Student,

    /// <summary>Učitel - může vytvářet a spravovat skupiny.</summary>
    Teacher,

    /// <summary>Administrátor - plný přístup do systému.</summary>
    Admin
}
