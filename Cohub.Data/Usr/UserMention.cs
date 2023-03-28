using System;
using System.Collections.Generic;

#nullable disable

namespace Cohub.Data.Usr
{
    public class UserMention : ICommentReference
    {
        public UserMention() { }
        public UserMention(User user)
        {
            User = user;
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int CommentId { get; set; }
        public bool Unread { get; set; }

        public virtual User User { get; set; }
        public virtual Comment Comment { get; set; }

        void ICommentReference.ReferenceComment(Comment comment)
        {
            Comment = comment;
        }

        public override string ToString()
        {
            return User?.ToString() ?? $"User:{UserId}";
        }
    }
}
