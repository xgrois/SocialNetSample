using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialNetSample.Data;

public class BlockedUser
{
    public Guid Id { get; set; }

    // FK (who initially blocked)
    public Guid SourceId { get; set; }
    public User Source { get; set; } = default!;

    // FK (who received the blocking)
    public Guid TargetId { get; set; }
    public User Target { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}

public class BlockedUserEntityTypeConfiguration : IEntityTypeConfiguration<BlockedUser>
{
    public void Configure(EntityTypeBuilder<BlockedUser> builder)
    {
        // Constraints
        builder.HasKey(x => x.Id);

        // (a,a) is not valid
        builder.HasCheckConstraint("BlockedUsers_SourceId_neq_TargetId", @$"[{nameof(BlockedUser.SourceId)}] != [{nameof(BlockedUser.TargetId)}]");

        /* BIDIRECTIONAL RELATIONSHIP */
        builder.Property<Guid>("UserIdA")
            .HasComputedColumnSql("case when SourceId < TargetId then SourceId else TargetId end");
        builder.Property<Guid>("UserIdB")
            .HasComputedColumnSql("case when SourceId < TargetId then TargetId else SourceId end");

        builder.HasIndex("UserIdA", "UserIdB").IsUnique();

        // On new row, use SQL Server SYSDATETIMEOFFSET() for CreatedAt column
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");


        /* NAV RELATIONSHIPS */
        // Source user has 0+ blocked users
        builder.HasOne(bu => bu.Source)
            .WithMany(u => u.BlockedUsersSource)
            .HasForeignKey(bu => bu.SourceId)
            .OnDelete(DeleteBehavior.NoAction);

        // Target user has 0+ blocked users
        builder.HasOne(bu => bu.Target)
            .WithMany(u => u.BlockedUsersTarget)
            .HasForeignKey(bu => bu.TargetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}