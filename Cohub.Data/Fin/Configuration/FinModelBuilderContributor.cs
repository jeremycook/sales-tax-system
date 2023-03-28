using Microsoft.EntityFrameworkCore;
using SiteKit.EntityFrameworkCore;

namespace Cohub.Data.Fin.Configuration
{
    public class FinModelBuilderContributor : IModelBuilderContributor
    {
        public void ContributeTo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Batch>().HasIndex(o => o.Name).IsUnique();
            modelBuilder.Entity<BatchComment>();
            modelBuilder.Entity<Category>();
            {
                modelBuilder.Entity<Category>().Property(o => o.TypeId).HasConversion<string>();
            }
            modelBuilder.Entity<Frequency>();
            modelBuilder.Entity<Bucket>();
            modelBuilder.Entity<PaymentConfiguration>();
            modelBuilder.Entity<Filing>();
            {
                modelBuilder.Entity<Filing>().HasDiscriminator(o => o.TypeId);
                modelBuilder.Entity<Filing>().HasIndex(o => o.FilingDate);
                modelBuilder.Entity<Filing>().HasIndex(o => o.Created);
                // Implementations
                modelBuilder.Entity<AssessmentFiling>();
                modelBuilder.Entity<FeeFiling>();
                modelBuilder.Entity<TaxFiling>();
            }
            modelBuilder.Entity<FilingSchedule>();
            {
                modelBuilder.Entity<FilingSchedule>().HasIndex(o => new { o.OrganizationId, o.PaymentChartId, o.StartDate }).IsUnique();
            }
            modelBuilder.Entity<PaymentChart>();
            modelBuilder.Entity<Period>();
            {
                modelBuilder.Entity<Period>().HasIndex(o => o.DueDate);
                modelBuilder.Entity<Period>().HasIndex(o => o.StartDate);
                modelBuilder.Entity<Period>().HasIndex(o => o.EndDate);
            }
            modelBuilder.Entity<Return>();
            {
                modelBuilder.Entity<Return>().HasIndex(o => new { o.OrganizationId, o.CategoryId, o.PeriodId }).IsUnique();
                modelBuilder.Entity<Return>()
                    .HasMany(p => p.Labels)
                    .WithMany(p => p.Returns)
                    .UsingEntity<ReturnLabel>(
                        j => j.HasOne(pt => pt.Label).WithMany().HasForeignKey(pt => pt.LabelId),
                        j => j.HasOne(pt => pt.Return).WithMany().HasForeignKey(pt => pt.ReturnId),
                        j =>
                        {
                            j.Property(pt => pt.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
                            j.HasKey(t => new { t.ReturnId, t.LabelId });
                        });

            }
            modelBuilder.Entity<ReturnComment>();
            modelBuilder.Entity<ReturnStatus>();
            {
                modelBuilder.Entity<ReturnStatus>().Property(o => o.Id).HasConversion<string>();
            }
            modelBuilder.Entity<Category>();
            modelBuilder.Entity<Subcategory>();
            modelBuilder.Entity<Statement>();
            {
                modelBuilder.Entity<Statement>().Property(o => o.TypeId).HasConversion<string>();
                modelBuilder.Entity<Statement>().Property(o => o.StatusId).HasConversion<string>();
            }
            modelBuilder.Entity<StatementComment>();
            modelBuilder.Entity<StatementDue>();
            modelBuilder.Entity<StatementReasonCode>();
            modelBuilder.Entity<Transaction>();
            modelBuilder.Entity<TransactionDetail>();
            {
                modelBuilder.Entity<TransactionDetail>().HasOne(o => o.Bucket).WithMany(o => o.TransactionDetails).OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<TransactionDetail>().HasOne(o => o.Category).WithMany(o => o.TransactionDetails).OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<TransactionDetail>().HasOne(o => o.Subcategory).WithMany(o => o.TransactionDetails).OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<TransactionDetail>().HasOne(o => o.Organization).WithMany(o => o.TransactionDetails).OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<TransactionDetail>().HasOne(o => o.Period).WithMany(o => o.TransactionDetails).OnDelete(DeleteBehavior.Restrict);
            }
        }
    }
}
