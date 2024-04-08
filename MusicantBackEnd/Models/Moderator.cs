using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicantBackEnd.Models
{
    public class Moderator
    {
        [Key] public int Id { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")] public virtual AppUser? User { get; set; }
        public int? CommunityId { get; set; }
        [ForeignKey("CommunityId")] public virtual Community? Community { get; set; }
    }
}
