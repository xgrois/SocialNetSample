using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialNetSample.Data;

public class Friend
{
    public Guid Id { get; set; }

    // FK (who sent initial friendship request)
    public Guid SourceId { get; set; }
    public User Source { get; set; } = default!;

    // FK (who received initial friendship request)
    public Guid TargetId { get; set; }
    public User Target { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}

public class FriendEntityTypeConfiguration : IEntityTypeConfiguration<Friend>
{
    public void Configure(EntityTypeBuilder<Friend> builder)
    {
        // Constraints
        builder.HasKey(x => x.Id);

        // (a,a) is not valid
        builder.HasCheckConstraint("Friends_SourceId_neq_TargetId", @$"[{nameof(Friend.SourceId)}] != [{nameof(Friend.TargetId)}]");

        /* BIDIRECTIONAL RELATIONSHIP */
        builder.Property<Guid>("UserIdA")
            .HasComputedColumnSql("case when SourceId < TargetId then SourceId else TargetId end");
        builder.Property<Guid>("UserIdB")
            .HasComputedColumnSql("case when SourceId < TargetId then TargetId else SourceId end");

        builder.HasIndex("UserIdA", "UserIdB").IsUnique();

        // On new row, use SQL Server SYSDATETIMEOFFSET() for CreatedAt column
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");


        /* NAV RELATIONSHIPS */
        // Source user has 0+ friends
        builder.HasOne(f => f.Source)
            .WithMany(u => u.FriendsSource)
            .HasForeignKey(f => f.SourceId)
            .OnDelete(DeleteBehavior.NoAction);

        // Target user has 0 + friends
        builder.HasOne(f => f.Target)
            .WithMany(u => u.FriendsTarget)
            .HasForeignKey(f => f.TargetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}