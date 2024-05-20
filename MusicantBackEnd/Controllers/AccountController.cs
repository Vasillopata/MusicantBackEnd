using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicantBackEnd.Data;
using MusicantBackEnd.Models;

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
        [HttpGet("getRecentFans")]
        public async Task<IActionResult> GetRecentFans()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }

            List<AppUser?> recentFans = await _db.Follows.Where(f => f.FollowedId == user.Id)
                .TakeLast(5)
                .Include(f => f.Follower)
                .Select(f => f.Follower)
                .ToListAsync();

            return new JsonResult(new {recentFans});
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
        [HttpGet("getAccount")]
        public async Task<IActionResult> GetAccount(int appUserId)
        {
            var appUser = await _db.AppUsers.FindAsync(appUserId);
            if (appUser == null) { return NotFound(); }

            return new JsonResult(new { appUser });
        }
    }
}
