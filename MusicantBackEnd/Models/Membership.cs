using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicantBackEnd.Models
{
    public class Membership
    {
        [Key] public int Id { get; set; }
        [Required] public int UserId { get; set; }
        [ForeignKey("UserId")] public virtual AppUser? User { get; set; }
        public int? CommunityId { get; set; }
        [ForeignKey("CommunityId")] public virtual Community? Community { get; set; }
    }
}
