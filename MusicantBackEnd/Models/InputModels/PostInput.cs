using Microsoft.AspNetCore.Mvc;

namespace MusicantBackEnd.Models.InputModels
{
    public class PostInput
    {
        public string Title { get; set; }
        public string? Text { get; set; }
        public byte[]? Image { get; set; }
        public int? CommunityId { get; set; }
    }
}
