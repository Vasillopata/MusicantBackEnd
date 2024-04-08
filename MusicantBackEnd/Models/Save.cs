using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MusicantBackEnd.Models
{
    [Keyless]
    public class Save
    {
        public int? UserId { get; set; }
        [ForeignKey("UserId")] public virtual AppUser? User { get; set; }
        public int? PostId { get; set; }
        [ForeignKey("PostId")] public virtual Post? Post { get; set; }
    }
}
