using System.Drawing;
using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public sealed class BoardisDbContext : DbContext
{
    public BoardisDbContext(DbContextOptions<BoardisDbContext> options) : base(options) { }
    
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
    public DbSet<BoardList> BoardLists => Set<BoardList>();
    public DbSet<ListCard> ListCards => Set<ListCard>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<MemberPermission> MemberPermissions => Set<MemberPermission>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // ------ Board -------
        builder.Entity<Board>(b =>
        {
            b.HasKey(board => board.Id);
            b.Property(board => board.Title)
                .IsRequired().HasMaxLength(100);
            
            b.Property(board => board.Description).HasMaxLength(500);
            
            b.Property(board => board.WallpaperImageId);

            b.Property(board => board.Visibility)
                .IsRequired()
                .HasDefaultValue(Domain.Constants.VisibilityLevel.Private)
                .HasConversion<string>(); // enum to string

            b.Property(board => board.CreatedAt).HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd(); // auto-set on insert
            
            b.Property(board => board.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate();
            
            // Relationships
            b.HasMany(board => board.Members)
                .WithOne() // No navigation property in BoardMember back to Board
                .HasForeignKey(member => member.BoardId)
                .OnDelete(DeleteBehavior.Cascade); // deleting a board deletes its members
            
            b.HasMany(board => board.BoardLists)
                .WithOne()
                .HasForeignKey(list => list.BoardId)
                .OnDelete(DeleteBehavior.Cascade); // deleting a board deletes its lists
            
        });
        
        // ------ BoardMember -------
        builder.Entity<BoardMember>(bm =>
        {
            bm.HasKey(b => new { b.BoardId, b.UserId }); // Composite key
            
            bm.Property(b => b.UserId).IsRequired();

            bm.Property(b => b.RoleId).IsRequired();
            
            bm.HasOne<Role>() // BoardMember has one Role
                .WithMany()   // Role can have many BoardMembers (no navigation property on Role)
                .HasForeignKey(b => b.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a Role if it's assigned to a member
            
            bm.HasMany(b => b.Permissions)
                .WithOne()
                .HasForeignKey(mp => new { mp.BoardId, mp.BoardMemberId })
                .OnDelete(DeleteBehavior.Cascade); // delete permissions of this member if member removed
                
            bm.Property(b => b.JoinedAt).HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();
            
            bm.Property(b => b.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate();
            
            bm.HasIndex(b => b.UserId); // Index on UserId for faster lookups
        });
        
        // ------ Role -------
        builder.Entity<Role>(r =>
        {
            r.HasKey(role => role.Id);
            r.Property(role => role.Key).HasMaxLength(256).IsRequired();
        });
        
        // ------ RolePermission -------
        builder.Entity<MemberPermission>(bm =>
        {
            bm.HasKey(r => r.Id);

            bm.Property(r => r.Permission)
                .IsRequired()
                .HasConversion<string>();
        });
        
        
        // // ------ BoardList -------
        builder.Entity<BoardList>(bl =>
        {
            bl.HasKey(list => list.Id);
            bl.Property(list => list.Title)
                .IsRequired().HasMaxLength(100);
            
            bl.Property(list => list.BoardId)
                .IsRequired();
            
            bl.Property(list => list.Position)
                .IsRequired().HasDefaultValue(0);
            
            bl.Ignore(list => list.ListColorArgb); // Ignore backing field
            bl.Property(list => list.ListColor)
                .IsRequired()
                .HasConversion(
                    v => v.ToArgb(), // Color to int
                    v => Color.FromArgb(v) // int to Color
                );

            bl.Property(list => list.CreatedAt)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();
            
            bl.Property(list => list.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate();

            // Index for sorting by Position
            bl.HasIndex(list => new { list.BoardId, list.Position });
        });
        
        // ------ Card -------
        builder.Entity<ListCard>(c =>
        {
            c.HasKey(card => card.Id);
            c.Property(card => card.BoardListId)
                .IsRequired();
            
            c.Property(card => card.Title)
                .IsRequired().HasMaxLength(100);
            
            c.Property(card => card.Description)
                .HasMaxLength(500);
            
            c.Property(card => card.CompletedAt);

            c.Property(card => card.Position)
                .IsRequired().HasDefaultValue(1024.0); // def pos to allow easy insertions

            c.Property(card => card.CreatedAt)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();
            
            c.Property(card => card.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate();

            // Index for sorting by Pos
            c.HasIndex(card => new { ListId = card.BoardListId, card.Position });
        });
        
        SeedRolesAndPermissions(builder);
    }
    
    private static void SeedRolesAndPermissions(ModelBuilder builder)
    {
        var ownerRoleId = new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef");
        var memberRoleId = new Guid("fedcba98-7654-3210-fedc-ba9876543210");

        builder.Entity<Role>().HasData(
            new { Id = ownerRoleId, Key = "Owner" },
            new { Id = memberRoleId, Key = "Member" }
        );
    }
}