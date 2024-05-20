using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MusicantBackEnd.Models
{
    public class Post
    {
        [Key] public int Id { get; set; }
        [Required] public string Title { get; set; }
        public string? Text { get; set; }
        public byte[]? Image { get; set; }
        [Required] public int UserId { get; set; }
        [ForeignKey("UserId")][JsonIgnore] public virtual AppUser User { get; set; }
        public int? CommunityId { get; set; }
        [ForeignKey("CommunityId")][JsonIgnore] public virtual Community? Community { get; set; }
    }
}
