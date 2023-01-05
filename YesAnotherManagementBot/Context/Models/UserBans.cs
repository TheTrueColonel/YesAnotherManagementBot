namespace YAMB.Context.Models; 

public sealed class UserBans {
    public int BanId { get; set; }
    public long UserId { get; set; }
    public DateTime BannedAt { get; set; }
    public DateTime UnbannedAt { get; set; }
}