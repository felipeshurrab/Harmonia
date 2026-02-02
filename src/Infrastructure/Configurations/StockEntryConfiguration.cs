using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations
{
    public class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
    {
        public void Configure(EntityTypeBuilder<StockEntry> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Quantity)
                .IsRequired();

            builder.Property(s => s.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.EntryDate)
                .IsRequired();

            builder.HasOne(s => s.Product)
                .WithMany(p => p.StockEntries)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.CreatedBy)
                .WithMany()
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
