using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicantBackEnd.Data;
using MusicantBackEnd.Models;
using MusicantBackEnd.Models.InputModels;
using System;
using System.IO.Compression;
using System.Linq;

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
            if (formData.ContainsKey("CommunityId"))
            {
                communityId = int.Parse(formData["CommunityId"]);
            }
            var post = new Post
            {
                Title = formData["Title"],
                Text = formData.ContainsKey("Text") ? formData["Text"].ToString() : null,
                CommunityId = communityId,
                UserId = user.Id,
                User = user,
                DatePosted = DateTime.UtcNow
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
            string? image = null;
            if (post.Image != null)
            {
                image = Convert.ToBase64String(post.Image);
            }
            return new JsonResult(new { id, title, text, userId, communityId, image });
        }
        [HttpGet("getPostsByUser")]
        public async Task<IActionResult> GetPostsByUser(int userId, int initPost, int amount)
        {
            if (userId == null || userId == 0) { return NotFound(); }
            List<int> posts = await _db.Posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Id)
                .Select(p => p.Id)
                .ToListAsync();

            return new JsonResult(new { posts });
        }
        [HttpGet("getPostsByCommunity")]
        public async Task<IActionResult> GetPostsByCommunity(int communityId, int initPost, int amount)
        {
            if (communityId == null || communityId == 0) { return NotFound(); }
            List<int> posts = await _db.Posts
                .Where(p => p.CommunityId == communityId)
                .OrderByDescending(p => p.Id)
                .Select(p => p.Id)
                .ToListAsync();

            return new JsonResult(new { posts });
        }
        [HttpGet("getPostsOnFeed")]
        public async Task<IActionResult> GetPostsOnFeed()
        {
            DateTime today = DateTime.UtcNow;
            var posts = await _db.Posts
                .Where(p => p.DatePosted.Date >= today.AddDays(-3).Date)
                .OrderByDescending(p => p.Id)
                .Select(p => p.Id)
                .ToListAsync();

            //var user = await _userManager.GetUserAsync(User);
            //var userId = user.Id;
            //DateTime today = DateTime.UtcNow;
            //var follows = await _db.Follows
            //	.Where(f => f.FollowerId == userId)
            //	.Select(f => f.FollowedId)
            //	.ToListAsync();
            //var memberships = await _db.Memberships
            //	.Where(m => m.UserId == userId)
            //	.Select(m => m.CommunityId)
            //	.ToListAsync();
            //var posts = await _db.Posts
            //	.Where(p => p.DatePosted.Date == today.Date && (follows.Contains(p.UserId) || memberships.Contains((int)p.CommunityId)))
            //	.OrderByDescending(p => p.Id)
            //  .Select(p => p.Id)
            //	.ToListAsync();

            return new JsonResult(new { posts });
        }
        [HttpPost("deletePost")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _db.Posts.FindAsync(postId);
            if (post == null) { return NotFound(); }
            var likes = await _db.Likes.Where(l => l.PostId == postId).ToListAsync();
            if (likes.Count>0)
            {
                _db.Likes.RemoveRange(likes);
            }

            var saves = await _db.Saves.Where(s => s.PostId == postId).ToListAsync();
            if (saves.Count>0)
            {
                _db.Saves.RemoveRange(saves);
            }

            var comments = await _db.Comments.Where(c => c.PostId == postId).ToListAsync();
            if (comments.Count>0)
            {
                _db.Comments.RemoveRange(comments);
            }

            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("likePost")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var post = await _db.Posts.FindAsync(postId);
            if (post == null) { return NotFound(); }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }

            Like like = new Like
            {
                PostId = post.Id,
                Post = post,
                UserId = user.Id,
                User = user
            };

            await _db.Likes.AddAsync(like);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("savePost")]
        public async Task<IActionResult> SavePost(int postId)
        {
            var post = await _db.Posts.FindAsync(postId);
            if (post == null) { return NotFound(); }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }

            Save save = new Save
            {
                PostId = post.Id,
                Post = post,
                UserId = user.Id,
                User = user
            };

            await _db.Saves.AddAsync(save);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("unlikePost")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var post = await _db.Posts.FindAsync(postId);
            if (post == null) { return NotFound(); }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }

            var like = await _db.Likes
                .Where(l => l.PostId == postId && l.UserId == user.Id)
                .FirstOrDefaultAsync();
            if (like == null) { return NotFound(); }

            _db.Likes.Remove(like);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("unsavePost")]
        public async Task<IActionResult> UnsavePost(int postId)
        {
            var post = await _db.Posts.FindAsync(postId);
            if (post == null) { return NotFound(); }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }

            var save = await _db.Saves
                .Where(s => s.PostId == postId && s.UserId == user.Id)
                .FirstOrDefaultAsync();
            if (save == null) { return NotFound(); }

            _db.Saves.Remove(save);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("comment")]
        public async Task<IActionResult> Comment([FromBody] CommentInput input)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }

            Comment comment = new Comment
            {
                UserId = user.Id,
                User = user,
                Text = input.Text,
                DatePosted = DateTime.UtcNow
            };
            if (input.ParentCommentId != null)
            {
                var parentComment = await _db.Comments.FindAsync(input.ParentCommentId);
                if (parentComment == null) { return NotFound(); }
                comment.ParentCommentId = parentComment.Id;
                comment.ParentComment = parentComment;
            }
            if (input.PostId != null)
            {
                var post = await _db.Posts.FindAsync(input.PostId);
                if (post == null) { return NotFound(); }
                comment.PostId = post.Id;
                comment.Post = post;
            }

            await _db.Comments.AddAsync(comment);
            await _db.SaveChangesAsync();

            return new JsonResult(new {comment});
        }
        [HttpPost("deletecomment")]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            var comment = await _db.Comments.FindAsync(commentId);
            if (comment == null) { return NotFound(); }

            comment.Deleted = true;
            await _db.SaveChangesAsync();
            return Ok(comment.Id);
        }
        [HttpGet("checkIfLiked")]
        public async Task<IActionResult> CheckIfLiked(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }
            var like = await _db.Likes
                .Where(l => l.PostId == postId && l.UserId == user.Id)
                .FirstOrDefaultAsync();
            if (like == null) { return Ok("false"); }
            else { return Ok("true"); }
        }
        [HttpGet("checkIfSaved")]
        public async Task<IActionResult> CheckIfSaved(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return BadRequest(); }
            var save = await _db.Saves
                .Where(s => s.PostId == postId && s.UserId == user.Id)
                .FirstOrDefaultAsync();
            if (save == null) { return Ok("false"); }
            else { return Ok("true"); }
        }
        [HttpGet("getCommentsByPost")]
        public async Task<IActionResult> GetCommentsByPost(int postId)
        {
            var comments = await _db.Comments
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Include(c => c.User)
                .OrderByDescending(c => c.DatePosted)
                .ToListAsync();
            var repliesPerComment = new List<int>();
            foreach (var comment in comments)
            {
                var replies = await _db.Comments
                    .Where(c => c.ParentCommentId == comment.Id)
                    .CountAsync();
                repliesPerComment.Add(replies);
            }

            return new JsonResult(new { comments, repliesPerComment });
        }
        [HttpGet("getRepliesByComment")]
        public async Task<IActionResult> GetRepliesByComment(int commentId)
        {
            var replies = await _db.Comments
                .Where(c => c.ParentCommentId == commentId)
                .Include(c => c.User)
                .OrderBy(c => c.DatePosted)
                .ToListAsync();
            var repliesPerReply = new List<int>();
            foreach (var reply in replies)
            {
                var repliesCount = await _db.Comments
                    .Where(c => c.ParentCommentId == reply.Id)
                    .CountAsync();
                repliesPerReply.Add(repliesCount);
            }

            return new JsonResult(new { replies, repliesPerReply });
        }
        [HttpGet("getCommentById")]
        public async Task<IActionResult> GetCommentById(int commentId)
        {
            var comment = await _db.Comments
                .Where(c => c.Id == commentId)
                .Include(c => c.User)
                .FirstOrDefaultAsync();
            var repliesCount = await _db.Comments
                .Where(c => c.ParentCommentId == commentId)
                .CountAsync();  
            if (comment == null) { return NotFound(); }

            return new JsonResult(new { comment, repliesCount });
        }

        [HttpGet("getLikedPosts")]
        public async Task<IActionResult> GetLikedPosts()
        {
            var user = await _userManager.GetUserAsync(User);

            List<int> likes = await _db.Likes
                .Where(l => l.UserId == user.Id)
                .Include(l => l.Post)
                .Select(l => l.Post.Id)
                .ToListAsync();

            return new JsonResult(new { likes });
        }
        [HttpGet("getSavedPosts")]
        public async Task<IActionResult> GetSavedPosts()
        {
            var user = await _userManager.GetUserAsync(User);

            List<int> saves = await _db.Saves
                .Where(l => l.UserId == user.Id)
                .Include(l => l.Post)
                .Select(l => l.Post.Id)
                .ToListAsync();

            return new JsonResult(new { saves });
        }
    }
}
