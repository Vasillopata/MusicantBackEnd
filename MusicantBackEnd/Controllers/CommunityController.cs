using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicantBackEnd.Data;
using MusicantBackEnd.Models;
using MusicantBackEnd.Models.InputModels;

namespace MusicantBackEnd.Controllers
{
    [ApiController]
    [Route("community")]
    public class CommunityController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;
        public CommunityController(UserManager<AppUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpPost("createCommunity")]
        public async Task<IActionResult> CreateCommunity(CommunityInput model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }

            var community = new Community
            {
                Name = model.Name,
                Type = model.Type,
                Description = model.Description,
                OwnerId = user.Id,
                Owner = user
            };
            await _db.AddAsync(community);
            await _db.SaveChangesAsync();
            Membership membership = new Membership
            {
                UserId = user.Id,
                CommunityId = community.Id,
                User = user,
                Community = community
            };
            await _db.Memberships.AddAsync(membership);
            await _db.SaveChangesAsync();

            return new JsonResult(new { community });
        }
        [HttpGet("getUserCommunities")]
        public async Task<IActionResult> GetUserCommunities()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }

            var communities = await _db.Memberships
                .Where(cm => cm.UserId == user.Id)
                .Include(cm => cm.Community)
                .Select(cm => cm.Community)
                .ToListAsync();


            return new JsonResult(new { communities });
        }

        [HttpGet("getCommunityById")]
        public async Task<IActionResult> GetCommunityById(int communityId)
        {
            var community = await _db.Communities.FindAsync(communityId);
            if (community == null) { return NotFound(); }

            return new JsonResult(new { community });
        }

        [HttpPost("becomeMember")]
        public async Task<IActionResult> BecomeMember(int communityId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound();}

            var community = await _db.Communities.FindAsync(communityId);
            if (community == null) { return NotFound();}

            Membership membership = new Membership
            {
                UserId = user.Id,
                CommunityId = communityId,
                User = user,
                Community = community
            };
            await _db.Memberships.AddAsync(membership);
            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpPost("endMembership")]
        public async Task<IActionResult> EndMembership(int communityId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound(); }

            var membership = await _db.Memberships
                .Where(m => m.UserId == user.Id && m.CommunityId == communityId)
                .FirstOrDefaultAsync();
            if (membership == null) {
                return Ok();
            } else {
                _db.Memberships.Remove(membership);
                await _db.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpGet("checkIfMember")]
        public async Task<IActionResult> CheckIfMember(int communityId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound(); }

            var membership = await _db.Memberships
                .Where(m => m.UserId == user.Id && m.CommunityId == communityId)
                .FirstOrDefaultAsync();

            if (membership == null)
                return Ok("false");
            else
                return Ok("true");
        }
        [HttpPost("makeModerator")]
        public async Task<IActionResult> MakeModerator(int userId,  int communityId)
        {
            var user = await _db.Users.FindAsync(userId);
            var community = await _db.Communities.FindAsync(communityId);
            Moderator moderator = new Moderator
            {
                UserId = userId,
                CommunityId = communityId,
                User = user,
                Community = community
            };
            await _db.Moderators.AddAsync(moderator);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("removeModerator")]
        public async Task<IActionResult> RemoveModerator(int userId, int communityId)
        {
            var moderator = await _db.Moderators.Where(m => m.UserId == userId && m.CommunityId == communityId).FirstOrDefaultAsync();
            if (moderator == null) return Ok("false");

            _db.Moderators.Remove(moderator);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("deleteCommunity")]
        public async Task<IActionResult> DeleteCommunity(int communityId)
        {
            var community = await _db.Communities.FindAsync(communityId);

            var mods = await _db.Moderators.Where(m => m.CommunityId == communityId).ToListAsync();
            _db.Moderators.RemoveRange(mods);

            var memberships = await _db.Memberships.Where(s => s.CommunityId == communityId).ToListAsync();
            _db.Memberships.RemoveRange(memberships);

            _db.Communities.Remove(community);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
