namespace SocialNet.Api.Models;
public class FriendRequestInDto
{
    public Guid SourceId { get; set; }
    public Guid TargetId { get; set; }
}