using FlowBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Infrastructure.Data.Configurations;

public class CanvasConfiguration : IEntityTypeConfiguration<Canvas>
{
    public void Configure(EntityTypeBuilder<Canvas> builder)
    {
        builder.ToTable("Canvases");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasOne(c => c.Board)
            .WithMany(b => b.Canvases)
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Team)
            .WithMany(t => t.Canvases)
            .HasForeignKey(c => c.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.CreatedBy)
            .WithMany(u => u.CreatedCanvases)
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TeamId);
        builder.HasIndex(c => c.BoardId);
    }
}
