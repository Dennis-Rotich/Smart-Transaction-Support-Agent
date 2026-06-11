using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace TransactionService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext 
{
	public ApplicationDbContext(DbContextOptions options) : base(options) {}

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionLog> TransactionLogs { get; set; }
    public DbSet<Document> Documents { get; set; }
	public DbSet<KnowledgeArticle> KnowledgeArticles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Transaction Configuration
		modelBuilder.Entity<Transaction>(entity => {
			entity.HasKey(e => e.Id);
			entity.Property(e => e.MerchantReference).IsRequired().HasMaxLength(50);
			entity.HasIndex(e => e.MerchantReference).IsUnique();
			entity.Property(e => e.Amount).HasPrecision(18, 2);
			entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);

			entity.Property(e => e.Status)
			 .HasConversion(v => v.ToString(), v => (TransactionStatus)Enum.Parse(typeof(TransactionStatus), v))
			 .HasMaxLength(20);

			entity.Property(e => e.TransactionReference).HasMaxLength(100);
            entity.Property(e => e.OrderTrackingId).HasMaxLength(100);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);

            // Relationship: Transaction -> Many logs
            entity.HasMany(e => e.Logs)
			 .WithOne(l => l.Transaction)
			 .HasForeignKey(l => l.TransactionId)
			 .OnDelete(DeleteBehavior.Cascade);
		});

		// Transaction Log Configuration
		modelBuilder.Entity<TransactionLog>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Type)
			 .HasConversion(v => v.ToString(), v => (EventType)Enum.Parse(typeof(EventType), v))
			 .HasMaxLength(50);

			entity.Property(e => e.Message).IsRequired(false);
			entity.Property(e => e.ProviderResponseCode).HasMaxLength(50).IsRequired(false);
			entity.Property(e => e.ProviderResponseBody).IsRequired(false);
		});

		// Document Configuration
		modelBuilder.Entity<Document>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
			entity.Property(e => e.StoragePath).IsRequired();
			entity.Property(e => e.DocumentType).HasMaxLength(100).IsRequired(false);
		});

        // Knowledge Article Configuration
        modelBuilder.Entity<KnowledgeArticle>(entity =>
        {
            // Explicitly map to the MySQL table name
            entity.ToTable("KNOWLEDGE_BASE");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Excerpt)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Content)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAdd();
        });
    }
}