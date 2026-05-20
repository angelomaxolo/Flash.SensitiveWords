using Flash.SensitiveWords.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flash.SensitiveWords.Infrastructure.Data;

public sealed class SensitiveWordsDbContext : DbContext
{
    public SensitiveWordsDbContext(DbContextOptions<SensitiveWordsDbContext> options)
        : base(options)
    {
    }

    public DbSet<SensitiveWord> SensitiveWords { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SensitiveWordsDbContext).Assembly);
    }
}
