using Cohub.Data.Usr;

#nullable disable

namespace Cohub.Data.Fin
{
    public class ReturnComment : ICommentReference
    {
        public ReturnComment() { }
        public ReturnComment(Return @return)
        {
            Return = @return;
        }
        public ReturnComment(int returnId)
        {
            ReturnId = returnId;
        }

        public int Id { get; private set; }
        public int ReturnId { get; private set; }
        public int CommentId { get; private set; }

        public virtual Return Return { get; private set; }
        public virtual Comment Comment { get; private set; }

        void ICommentReference.ReferenceComment(Comment comment)
        {
            Comment = comment;
        }

        public override string ToString()
        {
            return Return?.ToString() ?? $"Return:{ReturnId}";
        }
    }
}
