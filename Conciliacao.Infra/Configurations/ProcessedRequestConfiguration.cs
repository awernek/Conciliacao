using Conciliacao.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conciliacao.Infrastructure.Persistence.Configurations
{
    public class ProcessedRequestConfiguration
        : IEntityTypeConfiguration<ProcessedRequest>
    {
        public void Configure(EntityTypeBuilder<ProcessedRequest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.IdempotencyKey)
                   .IsUnique();

            builder.Property(x => x.IdempotencyKey)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.ResultHash)
                   .IsRequired();
        }
    }
}