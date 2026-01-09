using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Data;

/// <summary>
/// Entity Framework Core DbContext pro aplikaci Student Progress Tracker.
/// Definuje databázové sety a konfiguraci entit pomocí Fluent API.
/// Podporuje SQL Server i SQLite podle konfigurace.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Inicializuje novou instanci ApplicationDbContext.
    /// </summary>
    /// <param name="options">Konfigurační options (provider, connection string).</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>Tabulka uživatelů (učitelé, administrátoři).</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Tabulka studijních skupin.</summary>
    public DbSet<Group> Groups => Set<Group>();

    /// <summary>Tabulka učebních cílů.</summary>
    public DbSet<Goal> Goals => Set<Goal>();

    /// <summary>Tabulka členů skupin (studentů).</summary>
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();

    /// <summary>Tabulka pokroků studentů u cílů.</summary>
    public DbSet<MemberProgress> MemberProgresses => Set<MemberProgress>();

    /// <summary>Tabulka chatových zpráv.</summary>
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    /// <summary>Tabulka žádostí o pomoc.</summary>
    public DbSet<HelpRequest> HelpRequests => Set<HelpRequest>();

    /// <summary>
    /// Konfigurace entit pomocí Fluent API.
    /// Definuje primární klíče, indexy, vztahy a omezení.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ========== USER ==========
        builder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.GoogleId).HasMaxLength(100);
            // Unikátní index na e-mail pro rychlé vyhledávání
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ========== GROUP ==========
        builder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.JoinCode).HasMaxLength(20).IsRequired();
            // Unikátní index na JoinCode pro rychlé připojování studentů
            entity.HasIndex(e => e.JoinCode).IsUnique();

            // Vztah k vlastníkovi - Restrict zabrání smazání učitele se skupinami
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedGroups)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== GOAL ==========
        builder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);

            // Kaskádové mazání - smaže cíle při smazání skupiny
            entity.HasOne(e => e.Group)
                .WithMany(g => g.Goals)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== GROUP MEMBER ==========
        builder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Nickname).HasMaxLength(100).IsRequired();
            entity.Property(e => e.HelpReason).HasMaxLength(500);

            // Kompozitní unikátní index - jedno zařízení může být ve skupině jen jednou
            entity.HasIndex(e => new { e.GroupId, e.DeviceId }).IsUnique();

            // Kaskádové mazání - smaže členy při smazání skupiny
            entity.HasOne(e => e.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== MEMBER PROGRESS ==========
        builder.Entity<MemberProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Kompozitní unikátní index - jeden pokrok na člena a cíl
            entity.HasIndex(e => new { e.MemberId, e.GoalId }).IsUnique();

            // Kaskádové mazání při smazání člena
            entity.HasOne(e => e.Member)
                .WithMany(m => m.Progresses)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict při smazání cíle - zabrání nekonzistenci
            entity.HasOne(e => e.Goal)
                .WithMany(g => g.MemberProgresses)
                .HasForeignKey(e => e.GoalId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== CHAT MESSAGE ==========
        builder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.AIAnalysisJson);  // JSON pro uložení AI analýzy

            // Kaskádové mazání při smazání skupiny
            entity.HasOne(e => e.Group)
                .WithMany(g => g.Messages)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict při smazání člena - zachová zprávy
            entity.HasOne(e => e.Member)
                .WithMany(m => m.Messages)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== HELP REQUEST ==========
        builder.Entity<HelpRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(1000);

            // Kaskádové mazání při smazání člena
            entity.HasOne(e => e.Member)
                .WithMany(m => m.HelpRequests)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // SetNull při smazání učitele - zachová žádost bez resolvera
            entity.HasOne(e => e.ResolvedBy)
                .WithMany(u => u.ResolvedHelpRequests)
                .HasForeignKey(e => e.ResolvedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
