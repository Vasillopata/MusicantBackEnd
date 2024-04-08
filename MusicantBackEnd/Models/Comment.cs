using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MusicantBackEnd.Models
{
    public class Comment
    {
        [Key] public int Id { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")] public virtual AppUser? User { get; set; }
        public int? PostId { get; set; }
        [ForeignKey("PostId")] public virtual Post? Post { get; set; }
        [Required] public string Text { get; set; }
        public int? ParentCommentId { get; set;}
        [ForeignKey("ParentCommentId")]  public virtual Comment ParentComment { get; set; }

    }
}
