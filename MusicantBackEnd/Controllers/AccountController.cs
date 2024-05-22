using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MusicantBackEnd.Data;
using MusicantBackEnd.Misc;
using MusicantBackEnd.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MusicantBackEnd.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;
        public AccountController(UserManager<AppUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpGet("getUserNameById")]
        public async Task<IActionResult> GetUserNameById(int userId)
        {
            var appUser = await _db.AppUsers.FindAsync(userId);
            if (appUser == null) { return NotFound(); }
            return Ok(appUser.UserName);
        }

        [HttpGet("getUserName")]
        public async Task<IActionResult> GetUserName()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {return BadRequest();}
            var userName = user.UserName;

            return Ok(userName);
        }
        [HttpPost("follow")]
        public async Task<IActionResult> Follow(int otherUserId)
        {
            var thisUser = await _userManager.GetUserAsync(User);
            if (thisUser == null) { return BadRequest(); }

            var otherUser = await _db.AppUsers.FindAsync(otherUserId);
            if (otherUser == null) { return NotFound(); }

            Follow follow = new Follow{
                FollowerId = otherUserId,
                FollowedId = thisUser.Id,
                Followed = thisUser,
                Follower = otherUser
            };

            await _db.Follows.AddAsync(follow);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("like")]
        public async Task<IActionResult> Like(int postId)
        {
            var thisUser = await _userManager.GetUserAsync(User);
            if (thisUser == null) { return BadRequest(); }

            var post = await _db.Posts.FindAsync(postId);
            if (post == null) { return NotFound(); }
            Like like = new Like {
                UserId = thisUser.Id,
                User = thisUser,
                PostId = postId,
                Post = post
            };
            await _db.Likes.AddAsync(like);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("save")]
        public async Task<IActionResult> Save(int postId)
        {
            var thisUser = await _userManager.GetUserAsync(User);
            if (thisUser == null) { return BadRequest(); }

            var post = await _db.Posts.FindAsync(postId);
            if (post == null) { return NotFound(); }
            Save save = new Save
            {
                UserId = thisUser.Id,
                User = thisUser,
                PostId = postId,
                Post = post
            };
            await _db.Saves.AddAsync(save);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("getOwnAccount")]
        public async Task<IActionResult> GetOwnAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }

            var id = user.Id;
            var userName = user.UserName;
            var email = user.Email;
            var createdDate = user.CreatedDate;
            var birthDate = user.BirthDate;
            bool isLocked = user.IsLocked;
            string? pfp = null;
            string? banner = null;
            if (user.Pfp  != null)
                pfp = Convert.ToBase64String(user.Pfp);
            if (user.Banner != null)
                banner = Convert.ToBase64String(user.Banner);

            return new JsonResult(new { id, userName, email, createdDate, birthDate, pfp, banner, isLocked });
        }   
        [HttpGet("getAccount")]
        public async Task<IActionResult> GetAccount(int appUserId)
        {
            var appUser = await _db.AppUsers.FindAsync(appUserId);
            if (appUser == null) { return NotFound(); }

            var id = appUser.Id;
            var userName = appUser.UserName;
            var email = appUser.Email;
            var createdDate = appUser.CreatedDate;
            var birthDate = appUser.BirthDate;
            bool isLocked = appUser.IsLocked;
            string? pfp = null;
            string? banner = null;
            if (appUser.Pfp != null)
                pfp = Convert.ToBase64String(appUser.Pfp);
            if (appUser.Banner != null)
                banner = Convert.ToBase64String(appUser.Banner);

            return new JsonResult(new { id, userName, email, createdDate, birthDate, pfp, banner, isLocked });
        }

        [HttpPost("followAccount")]
        public async Task<IActionResult> FollowAccount(int accountId)
        {
            var user = await _userManager.GetUserAsync(User);
            var accountToFollow = await _db.Users.FindAsync(accountId);
            if (user == null || accountToFollow == null) { return BadRequest(); }

            Follow follow = new Follow
            {
                FollowerId = user.Id,
                Follower = user,
                FollowedId = accountId,
                Followed = accountToFollow
            };
            await _db.Follows.AddAsync(follow);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("unfollowAccount")]
        public async Task<IActionResult> UnfollowAccount(int accountId)
        {
            var user = await _userManager.GetUserAsync(User);
            var accountToUnfollow = await _db.Users.FindAsync(accountId);
            if (user == null || accountToUnfollow == null) { return BadRequest(); }

            var follow = await _db.Follows
                .Where(f => f.FollowerId == user.Id && f.FollowedId == accountId)
                .FirstOrDefaultAsync();
            if (follow == null) { return Ok(); }

            _db.Follows.Remove(follow);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("checkIfFollowing")]
        public async Task<IActionResult> CheckIfFollowing(int accountId)
        {
            var user = await _userManager.GetUserAsync(User);

            var follow = await _db.Follows
                .Where(f => f.FollowerId == user.Id && f.FollowedId == accountId)
                .FirstOrDefaultAsync();

            if (follow == null) { return Ok("false"); }
            return Ok("true");
        }
        [HttpGet("getRecentFans")]
        public async Task<IActionResult> GetRecentFans()
        {
            var user = await _userManager.GetUserAsync(User);

            var recentFans = await _db.Follows
                .Where(f => f.FollowerId == user.Id)
                .OrderByDescending(f => f.Id)
                .Include(f => f.Followed)
                .Select(f => f.Followed)
                .Take(5)
                .ToListAsync();

            return new JsonResult(new { recentFans });
        }
        [HttpPost("setPfp")]
        public async Task<IActionResult> SetPfp([FromForm] IFormCollection formData)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return BadRequest();

            IFormFile formFile = formData.Files[0];
            using (var ms = new MemoryStream())
            {
                await formFile.CopyToAsync(ms);
                user.Pfp = ms.ToArray();
            }
            await _db.SaveChangesAsync();
            return File(user.Pfp, "image/jpeg");
        }
        [HttpPost("setBanner")]
        public async Task<IActionResult> SetBanner([FromForm] IFormCollection formData)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return BadRequest();

            IFormFile formFile = formData.Files[0];
            using (var ms = new MemoryStream())
            {
                await formFile.CopyToAsync(ms);
                user.Banner = ms.ToArray();
            }
            await _db.SaveChangesAsync();
            return File(user.Banner, "image/jpeg");
        }
        [HttpGet("getOwnPfp")]
        public async Task<IActionResult> GetOwnPfp()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return BadRequest();

            if (user.Pfp == null) return NotFound();

            return File(user.Pfp, "image/jpeg");
        }
        [HttpGet("getPfpById")]
        public async Task<IActionResult> GetPfpById(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return BadRequest();

            if (user.Pfp == null) return NotFound();

            return File(user.Pfp, "image/jpeg");
        }
        [HttpGet("checkIfOwner")]
        public async Task<IActionResult> CheckIfOwner(int communityId)
        {
            var user = await _userManager.GetUserAsync(User);
            var community = await _db.Communities.FindAsync(communityId);

            bool isOwner = (community.OwnerId == user.Id);
            return Ok(isOwner);
        }
        [HttpGet("checkIfAdmin")]
        public async Task<IActionResult> CheckIfAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, AppRoles.Admin);

            return Ok(isAdmin);
        }
        [HttpGet("checkIfModerator")]
        public async Task<IActionResult> CheckIfModerator(int communityId)
        {
            var user = await _userManager.GetUserAsync(User);

            var isModerator = await _db.Moderators.AnyAsync(m => m.UserId == user.Id && m.CommunityId == communityId);

            return Ok(isModerator);
        }
        [HttpPost("lockAccount")]
        public async Task<IActionResult> LockAccount(int userId)
        {
            var user = await _db.Users.FindAsync(userId);

            user.IsLocked = true;
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("unlockAccount")]
        public async Task<IActionResult> UnlockAccount(int userId)
        {
            var user = await _db.Users.FindAsync(userId);

            user.IsLocked = false;
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
