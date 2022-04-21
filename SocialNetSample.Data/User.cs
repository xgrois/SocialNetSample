using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialNetSample.Data;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public bool Deleted { get; set; }

    // Nav properties
    public List<FriendRequest> FriendRequestsSource { get; set; } = new();
    public List<FriendRequest> FriendRequestsTarget { get; set; } = new();

    public List<Friend> FriendsSource { get; set; } = new();
    public List<Friend> FriendsTarget { get; set; } = new();

    public List<BlockedUser> BlockedUsersSource { get; set; } = new();
    public List<BlockedUser> BlockedUsersTarget { get; set; } = new();
}

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Constraints
        builder.HasKey(x => x.Id); // PK

        builder.HasIndex(x => x.Email).IsUnique(); // (AK) Index and unique
        builder.HasQueryFilter(x => !x.Deleted); // This will affect all queries automatically avoiding "Deleted Users"

        builder.Property(x => x.Email).HasMaxLength(50);
        builder.Property(x => x.Deleted).HasDefaultValue(false);
    }
}