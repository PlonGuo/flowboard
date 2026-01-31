using FlowBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Infrastructure.Data.Configurations;

public class AIChatHistoryConfiguration : IEntityTypeConfiguration<AIChatHistory>
{
    public void Configure(EntityTypeBuilder<AIChatHistory> builder)
    {
        builder.ToTable("AIChatHistory");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.Response)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(a => a.ActionTaken)
            .HasMaxLength(100);

        builder.HasOne(a => a.User)
            .WithMany(u => u.ChatHistory)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Board)
            .WithMany(b => b.ChatHistory)
            .HasForeignKey(a => a.BoardId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.CreatedAt);
    }
}
