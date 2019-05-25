namespace TeamChat.DAL.Entities
{
    public class Comment : Activity
    {
        public Post BelongsTo { get; set; }
    }
}
