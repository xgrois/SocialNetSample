using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialNetSample.Data;

public class FriendRequest
{
    // Not strictly needed, but I prefer to use this for replying the id of the new FR
    // e.g.: created at "/friendrequests/id"
    public Guid Id { get; set; }

    // FK (who sent initial friendship request)
    public Guid SourceId { get; set; }
    public User Source { get; set; } = default!;

    // FK (who received initial friendship request)
    public Guid TargetId { get; set; }
    public User Target { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}

public class FriendRequestEntityTypeConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        // Constraints
        builder.HasKey(x => x.Id);

        // (a,a) is not valid
        builder.HasCheckConstraint("FriendRequests_SourceId_neq_TargetId", @$"[{nameof(FriendRequest.SourceId)}] != [{nameof(FriendRequest.TargetId)}]");

        /* BIDIRECTIONAL RELATIONSHIP */
        // NOTE: this can be done by directly normalizing FKs in different ways
        // (userA, userB, source, ...)
        //    2      4      4
        // but in this case the User navigation properties will be "inconsistent"
        // because List<Relationship> OutgoingRelationships will no longer have
        // only "source" relationships since userA can be source or target 
        // (the same applies for IncommingRelationships list)
        //
        // We specify two computed properties that normalize (SourceId,TargetId) order
        // If (sourceId=4,targetId=2) the normalized columns would be (2,4) since 2 < 4
        builder.Property<Guid>("UserIdA")
            .HasComputedColumnSql("case when SourceId < TargetId then SourceId else TargetId end");
        builder.Property<Guid>("UserIdB")
            .HasComputedColumnSql("case when SourceId < TargetId then TargetId else SourceId end");

        // By setting above generated columns as unique index, 
        // we grant that only a single relationship between (a,b) exists in the table
        // This prevents to add (a,b), if (b,a) already exists
        builder.HasIndex("UserIdA", "UserIdB").IsUnique();

        // When we add a new Friend Request, this column is automatically filled using SQL Server SYSDATETIMEOFFSET()
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");


        /* RELATIONSHIPS */
        // Source user has 0+ FRs
        builder.HasOne(fr => fr.Source)
            .WithMany(u => u.FriendRequestsSource)
            .HasForeignKey(fr => fr.SourceId)
            .OnDelete(DeleteBehavior.NoAction);

        // Target user has 0+ FRs
        builder.HasOne(fr => fr.Target)
            .WithMany(u => u.FriendRequestsTarget)
            .HasForeignKey(fr => fr.TargetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
