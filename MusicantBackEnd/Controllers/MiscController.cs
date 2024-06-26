﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicantBackEnd.Data;
using MusicantBackEnd.Models;

namespace MusicantBackEnd.Controllers
{
    [ApiController]
    [Route("misc")]
    public class MiscController : ControllerBase
    {
        private ILogger<MiscController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;
        public MiscController(AppDbContext db, UserManager<AppUser> userManager, ILogger<MiscController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }
        [HttpGet("search")] 
        public async Task<IActionResult> Search(string input)
        {
           _logger.LogInformation("Search: " + input);
            var user = await _userManager.GetUserAsync(User);
            List<AppUser> friends = await _db.Follows.Where(f => f.FollowedId == user.Id || f.FollowerId == user.Id)
                .Include(f => f.Followed)
                .Select(f => f.Followed)
                .Where(p => p.UserName.ToLower().Contains(input.ToLower()))
                .ToListAsync();
            List<AppUser> profiles = await _db.AppUsers
                .Where(p => p.UserName.ToLower().Contains(input.ToLower()))
                .ToListAsync();
            profiles = profiles.Except(friends).ToList();
            _logger.LogInformation("Profiles: " + profiles.Count);
            var posts = await _db.Posts
                .Where(p => p.Title.ToLower().Contains(input.ToLower()))
                .ToListAsync();
            var postImgs = new List<string>();
            foreach (Post post in posts)
            {
                if (post.Image != null)
                {
                    postImgs.Add(String.Concat("data:image/jpeg;base64,", Convert.ToBase64String(post.Image)));
                }
                else
                {
                    postImgs.Add("");
                }

            }
            var communities = await _db.Communities
                .Where(c => c.Name.ToLower().Contains(input.ToLower()))
                .ToListAsync();

            return new JsonResult(new {friends, profiles, posts, communities, postImgs});
        }
    }
}
