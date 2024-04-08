using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicantBackEnd.Models
{
    [Keyless]
    public class Follow
    {
        public int? FollowerId {  get; set; }
        [ForeignKey("FollowerId")] public virtual AppUser? Follower { get; set; }

        public int? FollowedId { get; set; }
        [ForeignKey("FollowedId")] public virtual AppUser? Followed { get; set; }
    }
}
