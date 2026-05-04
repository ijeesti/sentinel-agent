using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelAgent.Domain.Entities;

namespace SentinelAgent.Infrastructure.Persistence.Configurations;

public class FailureRecordConfiguration : IEntityTypeConfiguration<FailureRecord>
{
    public void Configure(EntityTypeBuilder<FailureRecord> builder)
    {

        builder.HasKey(x => x.Fingerprint);

        builder.Property(x => x.Fingerprint)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(256);

        builder.Property(x => x.OccurrenceCount)
            .IsRequired();

        builder.Property(x => x.FirstSeenUtc)
            .IsRequired();

        builder.Property(x => x.LastSeenUtc)
            .IsRequired();

        builder.HasIndex(x => x.Fingerprint)
            .IsUnique();

        builder.HasIndex(x => x.LastSeenUtc);

        //builder.ToTable("gre", "Failures");
    }
}