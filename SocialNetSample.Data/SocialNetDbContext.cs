using Microsoft.EntityFrameworkCore;

namespace SocialNetSample.Data;
public class SocialNetDbContext : DbContext
{
    public SocialNetDbContext(DbContextOptions<SocialNetDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Friend> Friends => Set<Friend>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<BlockedUser> BlockedUsers => Set<BlockedUser>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Instead of defining here all property tables and fields we just read all
        // from each IEntityTypeConfiguration<T> derived class
        // This way is crearer and prevents this method to grow
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SocialNetDbContext).Assembly);
    }
}
