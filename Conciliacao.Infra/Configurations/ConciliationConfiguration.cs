using Conciliacao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conciliacao.Infra.Configurations
{
    public class ConciliationConfiguration
        : IEntityTypeConfiguration<Conciliation>
    {
        public void Configure(EntityTypeBuilder<Conciliation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExternalReference)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Amount)
                   .HasPrecision(18, 2);

            builder.HasIndex(x => x.ExternalReference)
                   .IsUnique();
        }
    }
}
