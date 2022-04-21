namespace SocialNet.Api.Models;
public class FriendOutDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }


    public Guid SourceId { get; set; }
    public string SourceEmail { get; set; } = default!;
    public Guid TargetId { get; set; }
    public string TargetEmail { get; set; } = default!;
}