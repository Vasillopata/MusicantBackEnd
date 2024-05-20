using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicantBackEnd.Data;
using MusicantBackEnd.Models;
using MusicantBackEnd.Models.InputModels;
using System;
using System.IO.Compression;

namespace MusicantBackEnd.Controllers
{
    [ApiController]
    [Route("post")]
    public class PostController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;
        public PostController(UserManager<AppUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpPost("createPost")]
        public async Task<IActionResult> CreatePost([FromForm] IFormCollection formData)
        {
            var user = await _userManager.GetUserAsync(User);
            int? communityId = null;
            if(formData.ContainsKey("CommunityId"))
            {
                communityId = int.Parse(formData["CommunityId"]);
            }
            var post = new Post
            {
                Title = formData["Title"],
                Text = formData.ContainsKey("Text") ? formData["Text"].ToString() : null,
                CommunityId = communityId,
                UserId = user.Id,
                User = user
            };

            if (formData.Files.Count > 0)
            {
                IFormFile file = formData.Files[0];
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    post.Image = ms.ToArray();
                }
            }

            await _db.Posts.AddAsync(post);
            await _db.SaveChangesAsync();

            return Ok(post.Id);
        }

        [HttpGet("getPostImg")]
        public async Task<IActionResult> GetPostImg(int postId)
        {
            var post = await _db.Posts.FindAsync(postId);
            if (post == null) { return NotFound(); }

            return File(post.Image, "image/jpeg");
        }

        [HttpGet("getPostById")]
        public async Task<IActionResult> GetPostById(int postId)
        {
            var post = await _db.Posts.FindAsync(postId);
            if (post == null) { return NotFound(); }
            var id = post.Id;
            var title = post.Title;
            var text = post.Text;
            var userId = post.UserId;
            var communityId = post.CommunityId;

            return new JsonResult(new {id, title, text, userId, communityId});
        }
        [HttpGet("getPosts")]
        public async Task<IActionResult> GetPosts(int userId, int initPost, int amount, int communityId)
        {
            List<int> posts;
            if (userId != 0 && communityId == 0)
            {
                posts = await _db.Posts
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.Id)
                    .Skip(initPost - 1)
                    .Take(amount)
                    .Select(p => p.Id)
                    .ToListAsync();
            }
            else if (userId == 0 && communityId != 0)
            {
                posts = await _db.Posts
                    .Where(p => p.CommunityId == communityId)
                    .OrderByDescending(p => p.Id)
                    .Skip(initPost - 1)
                    .Take(amount)
                    .Select(p => p.Id)
                    .ToListAsync();
            }
            else if (userId != 0 && communityId != 0)
            {
                posts = await _db.Posts
                    .Where(p => p.UserId == userId && p.CommunityId == communityId)
                    .OrderByDescending(p => p.Id)
                    .Skip(initPost - 1)
                    .Take(amount)
                    .Select(p => p.Id)
                    .ToListAsync();
            }
            else
            {
                return BadRequest();
            }

            return new JsonResult(new { posts });
        }
        
    }
}
