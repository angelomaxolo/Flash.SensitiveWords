using Flash.SensitiveWords.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flash.SensitiveWords.Infrastructure.Data.Configurations;

public class SensitiveWordConfiguration : IEntityTypeConfiguration<SensitiveWord>
{
    public void Configure(EntityTypeBuilder<SensitiveWord> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Word)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.CreatedDate)
            .IsRequired();

        builder.HasIndex(e => e.Word)
            .IsUnique();
    }
}
