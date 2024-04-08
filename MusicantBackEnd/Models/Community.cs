using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicantBackEnd.Models
{
    public class Community
    {
        [Key]public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Type { get; set; }
        public string? Description { get; set; }
        [Required] public int OwnerId {  get; set; }
        [ForeignKey("OwnerId")] public virtual AppUser Owner { get; set; }
    }
}
