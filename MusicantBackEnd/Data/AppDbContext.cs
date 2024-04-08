using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using MusicantBackEnd.Models;

namespace MusicantBackEnd.Data
{
    public class AppDbContext : IdentityDbContext<AppUser,IdentityRole<int>,int> 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Moderator> Moderators { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Save> Saves { get; set; }
    }
}
