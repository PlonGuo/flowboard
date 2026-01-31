using FlowBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Infrastructure.Data.Configurations;

public class CanvasDataConfiguration : IEntityTypeConfiguration<CanvasData>
{
    public void Configure(EntityTypeBuilder<CanvasData> builder)
    {
        builder.ToTable("CanvasData");

        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.Elements)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(cd => cd.AppState)
            .HasColumnType("text");

        builder.Property(cd => cd.Files)
            .HasColumnType("text");

        builder.Property(cd => cd.Version)
            .HasDefaultValue(1);

        builder.HasOne(cd => cd.Canvas)
            .WithOne(c => c.Data)
            .HasForeignKey<CanvasData>(cd => cd.CanvasId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cd => cd.CanvasId);
    }
}
