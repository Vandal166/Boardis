using System.Drawing;
using Domain.Board.Entities;
using Domain.BoardLists.Entities;
using Domain.BoardMembers.Entities;
using Domain.Common;
using Domain.Images.Entities;
using Domain.ListCards.Entities;
using Domain.MemberPermissions.Entities;
using Domain.ValueObjects;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public sealed class BoardisDbContext : DbContext
{
    private readonly DomainEventInterceptor _domainEventInterceptor;
    public BoardisDbContext(DbContextOptions<BoardisDbContext> options, DomainEventInterceptor domainEventInterceptor) : base(options)
    {
        _domainEventInterceptor = domainEventInterceptor;
    }
    
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
    public DbSet<BoardList> BoardLists => Set<BoardList>();
    public DbSet<ListCard> ListCards => Set<ListCard>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<MemberPermission> MemberPermissions => Set<MemberPermission>();
    public DbSet<Media> Media => Set<Media>();
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
        
        optionsBuilder.AddInterceptors(_domainEventInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Ignore<List<IDomainEvent>>(); // Ignore domain events collection
        
        // ------ Board -------
        builder.Entity<Board>(b =>
        {
            b.HasKey(board => board.Id);
            b.Property(x => x.Id).ValueGeneratedNever();

            b.Property(board => board.Title)
                .HasConversion(new Title.EfCoreValueConverter())
                .HasMaxLength(100)
                .IsRequired();

            b.Property(board => board.Description)
                .HasMaxLength(500)
                .IsRequired(false);
            
            b.Property(board => board.Visibility)
                .IsRequired()
                .HasDefaultValue(Domain.Constants.VisibilityLevel.Private)
                .HasConversion<string>(); // enum to string

            b.Property(board => board.CreatedAt).HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd(); // auto-set on insert

            b.Property(board => board.UpdatedAt).IsRequired(false);
            
            // Relationships
            b.HasMany(board => board.Members)
                .WithOne() // No navigation property in BoardMember back to Board
                .HasForeignKey(member => member.BoardId)
                .OnDelete(DeleteBehavior.Cascade); // deleting a board deletes its members
            
            b.HasMany(board => board.Lists)
                .WithOne()
                .HasForeignKey(list => list.BoardId)
                .OnDelete(DeleteBehavior.Cascade); // deleting a board deletes its lists
            
            b.Navigation(board => board.Lists)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            
            b.Navigation(board => board.Members)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            
            // Index for faster lookups by title
            b.HasIndex(board => board.Title);
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
            
            bm.OwnsMany(b => b.Permissions, perm =>
            {
                perm.WithOwner().HasForeignKey("BoardId", "UserId"); // FK to BoardMember
                perm.Property(m => m.Permission)
                    .IsRequired()
                    .HasConversion<string>();
                
                perm.Property(p => p.GrantedAt)
                    .HasDefaultValueSql("now()");
                
                perm.HasKey("BoardId", "UserId", "Permission");// Composite PK to prevent duplicate permissions of the same type for a member
            });
            
            bm.Navigation(x => x.Permissions)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            
            bm.Property(b => b.JoinedAt).HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            bm.Property(b => b.UpdatedAt).IsRequired(false);
            
            bm.HasIndex(b => b.UserId); // Index on UserId for faster lookups
        });
        
        // ------ Role -------
        builder.Entity<Role>(r =>
        {
            r.HasKey(role => role.Id);
            r.Property(x => x.Id).ValueGeneratedNever();
            
            r.Property(role => role.Key).HasMaxLength(256).IsRequired();
        });
        
        // // ------ BoardList -------
        builder.Entity<BoardList>(bl =>
        {
            bl.HasKey(list => list.Id);
            bl.Property(x => x.Id).ValueGeneratedNever();
            
            bl.Property(list => list.BoardId)
                .IsRequired();
            
            bl.Property(list => list.Title)
                .HasConversion(new Title.EfCoreValueConverter())
                .HasMaxLength(100)
                .IsRequired();
            
            bl.Property(list => list.Position)
                .IsRequired().HasDefaultValue(1024.0);
            
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

            bl.Property(list => list.UpdatedAt).IsRequired(false);
            
            bl.HasMany(list => list.Cards)
                .WithOne()
                .HasForeignKey(card => card.BoardListId)
                .OnDelete(DeleteBehavior.Cascade); // deleting a list deletes its cards
            
            bl.Navigation(list => list.Cards)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // Index for sorting by Position
            bl.HasIndex(list => new { list.BoardId, list.Position });
        });
        
        // ------ Card -------
        builder.Entity<ListCard>(c =>
        {
            c.HasKey(card => card.Id);
            c.Property(x => x.Id).ValueGeneratedNever();
            
            c.Property(card => card.BoardListId)
                .IsRequired();
            
            c.Property(card => card.Title)
                .HasConversion(new Title.EfCoreValueConverter())
                .HasMaxLength(100)
                .IsRequired();

            c.Property(card => card.Description)
                .HasMaxLength(500)
                .IsRequired(false);
            
            c.Property(card => card.CompletedAt)
                .IsRequired(false);

            c.Property(card => card.Position)
                .IsRequired().HasDefaultValue(1024.0); // def pos to allow easy insertions

            c.Property(card => card.CreatedAt)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            c.Property(card => card.UpdatedAt).IsRequired(false);

            // Index for sorting by Pos
            c.HasIndex(card => new { ListId = card.BoardListId, card.Position });
        });
        
        // ------ Media -------
        builder.Entity<Media>(m =>
        {
            m.HasKey(media => media.Id);
            
            m.Property(media => media.BoundToEntityId)
                .IsRequired();
            
            m.Property(media => media.UploadedAt)
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();
            
            m.Property(media => media.UploadedByUserId)
                .IsRequired();
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