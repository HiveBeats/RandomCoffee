using Microsoft.EntityFrameworkCore;

namespace RandomCoffee;

public class CoffeeContext : DbContext
{
    public CoffeeContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Group> Groups { get; set; }
    public DbSet<Coffee> Coffees { get; set; }
    public DbSet<OutBoxMessage> OutBoxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>()
            .Property(x => x.Id).HasMaxLength(64);

        modelBuilder.Entity<Group>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<Group>()
            .HasMany<Coffee>(x => x.Coffees)
            .WithOne(x => x.Group)
            .HasForeignKey(x => x.GroupId)
            .HasPrincipalKey(x => x.Id);


        modelBuilder.Entity<Coffee>()
            .Property(x => x.Id);

        modelBuilder.Entity<Coffee>()
            .Property(x => x.GroupId).HasMaxLength(64);

        modelBuilder.Entity<Coffee>()
            .HasMany<Participant>(x => x.CoffeeParticipants)
            .WithOne(x => x.Coffee)
            .HasForeignKey(x => x.CoffeeId)
            .HasPrincipalKey(x => x.Id);

        modelBuilder.Entity<Coffee>()
            .HasKey(x => x.Id);

        // modelBuilder.Entity<Coffee>()
        //     .HasIndex(x => x.GroupId);
        modelBuilder.Entity<Coffee>()
            .HasIndex(x => x.AnnouncedAt);

        modelBuilder.Entity<Participant>()
            .Property(x => x.Id);

        modelBuilder.Entity<Participant>()
            .Property(x => x.UserName).HasMaxLength(128);

        modelBuilder.Entity<Participant>()
            .HasKey(x => x.Id);

        // modelBuilder.Entity<Participant>()
        //     .HasIndex(x => x.CoffeeId);
        modelBuilder.Entity<Participant>()
            .HasIndex(x => x.UserName);
        modelBuilder.Entity<Participant>()
            .HasIndex(x => x.ScheduledAt);


        modelBuilder.Entity<OutBoxMessage>()
            .Property(x => x.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<OutBoxMessage>()
            .Property(x => x.ChatId).HasMaxLength(64);
        modelBuilder.Entity<OutBoxMessage>()
            .Property(x => x.ReplyToMessageId).HasMaxLength(64);
        modelBuilder.Entity<OutBoxMessage>()
            .Property(x => x.Text).HasMaxLength(4096);
        modelBuilder.Entity<OutBoxMessage>()
            .Property(x => x.ParseMode)
            .HasConversion<int>();

        modelBuilder.Entity<OutBoxMessage>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<OutBoxMessage>()
            .HasIndex(x => x.CreatedAt);
        modelBuilder.Entity<OutBoxMessage>()
            .HasIndex(x => x.ProcessedAt);
    }
}