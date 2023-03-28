using System;

#nullable disable

namespace Cohub.Data.Fin
{
    public class StatementComment : Usr.ICommentReference
    {
        public StatementComment() { }
        public StatementComment(Statement statement)
        {
            Statement = statement;
        }
        public StatementComment(int statementId)
        {
            StatementId = statementId;
        }

        public int Id { get; private set; }
        public int StatementId { get; private set; }
        public int CommentId { get; private set; }
        public DateTimeOffset Created { get; private set; }

        public virtual Statement Statement { get; private set; }
        public virtual Usr.Comment Comment { get; private set; }

        void Usr.ICommentReference.ReferenceComment(Usr.Comment comment)
        {
            Comment = comment;
        }

        public override string ToString()
        {
            return Statement?.ToString() ?? $"Statement:{StatementId}";
        }
    }
}
