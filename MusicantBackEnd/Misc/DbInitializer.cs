using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicantBackEnd.Data;
using MusicantBackEnd.Models;

namespace MusicantBackEnd.Misc
{
    public class DbInitializer : IDbInitializer
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public DbInitializer(AppDbContext db, UserManager<AppUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initialize()
        {
            if (_db.Roles.Any(x => x.Name == AppRoles.Admin))
                return;

            _roleManager.CreateAsync(new IdentityRole<int>(AppRoles.Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole<int>(AppRoles.User)).GetAwaiter().GetResult();

            _userManager.CreateAsync(new AppUser
            {
                UserName = "Vasillopata",
                Email = "vasillopata@gmail.com",
                EmailConfirmed = true

            }, "Parola123?").GetAwaiter().GetResult();

            var user = _db.Users.FirstOrDefaultAsync(u => u.Email == "vasillopata@gmail.com").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(user, AppRoles.Admin).GetAwaiter().GetResult();
        }
    }
}
