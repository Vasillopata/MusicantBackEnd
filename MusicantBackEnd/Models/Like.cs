using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicantBackEnd.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")] public virtual AppUser? User { get; set; }
        public int? PostId { get; set; }
        [ForeignKey("PostId")] public virtual Post? Post { get; set; }
    }
}
