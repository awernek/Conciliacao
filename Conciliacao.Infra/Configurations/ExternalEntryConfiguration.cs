using Conciliacao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conciliacao.Infrastructure.Persistence.Configurations
{
    public class ExternalEntryConfiguration : IEntityTypeConfiguration<ExternalEntry>
    {
        public void Configure(EntityTypeBuilder<ExternalEntry> builder)
        {
            builder.ToTable("ExternalEntries");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Reference)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.Date)
                .IsRequired();

            builder.Property(e => e.Source)
                .HasMaxLength(100);
        }
    }
}