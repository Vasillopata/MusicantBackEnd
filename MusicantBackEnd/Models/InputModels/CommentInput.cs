namespace MusicantBackEnd.Models.InputModels
{
    public class CommentInput
    {
        public string Text { get; set; }
        public int? PostId { get; set; }
        public int? ParentCommentId { get; set; }

    }
}
