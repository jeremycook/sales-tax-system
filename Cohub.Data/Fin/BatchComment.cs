using Cohub.Data.Usr;
using System;
using System.Collections.Generic;

#nullable disable

namespace Cohub.Data.Fin
{
    public class BatchComment : ICommentReference
    {
        public BatchComment() { }
        public BatchComment(Batch batch)
        {
            Batch = batch;
        }
        public BatchComment(int batchId)
        {
            BatchId = batchId;
        }

        public int Id { get; private set; }
        public int BatchId { get; private set; }
        public int CommentId { get; private set; }
        public DateTimeOffset Created { get; private set; }
        public int CreatedById { get; private set; }

        public virtual Batch Batch { get; private set; }
        public virtual Comment Comment { get; private set; }
        public virtual User CreatedBy { get; private set; }

        void ICommentReference.ReferenceComment(Comment comment)
        {
            Comment = comment;
        }

        public override string ToString()
        {
            return Batch?.ToString() ?? $"Batch:{BatchId}";
        }
    }
}
