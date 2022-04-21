namespace SocialNet.Api.Models;
public class FriendRequestOutDto
{
    public Guid Id { get; set; }
    public Guid SourceId { get; set; }
    public Guid TargetId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}