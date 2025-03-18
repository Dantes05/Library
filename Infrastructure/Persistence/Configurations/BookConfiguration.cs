using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.ISBN)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(b => b.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(b => b.Genre)
               .HasMaxLength(100);

        builder.Property(b => b.Description)
               .HasMaxLength(500);

        builder.Property(b => b.TakenAt)
               .IsRequired(false);

        builder.Property(b => b.ReturnAt)
               .IsRequired(false);

        builder.Property(b => b.AuthorId)
               .IsRequired();

        builder.HasIndex(b => b.ISBN) 
               .IsUnique();
    }
}
