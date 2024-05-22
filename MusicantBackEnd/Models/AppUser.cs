using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace MusicantBackEnd.Models
{
    public class AppUser : IdentityUser<int>
    {
        [Key]
        public override int Id { get; set; }

        [Required]
        public override string UserName { get; set; }

        public byte[]? Pfp {  get; set; }

        public byte[]? Banner { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }
        public bool IsLocked { get; set; } = false;
    }
}
