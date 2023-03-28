using Cohub.Data.Fin;
using Humanizer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Usr
{
    public class Comment
    {
        public Comment()
        {
        }

        public Comment(params ICommentReference[] commentReferences)
        {
            foreach (var item in commentReferences)
            {
                item.ReferenceComment(this);
            }
        }

        public override string ToString()
        {
            return Text?.Truncate(25);
        }

        public int Id { get; set; }

        public bool IsUserComment { get; set; }

        [Display(Name = "Comment")]
        [DataType(DataType.Html)]
        public string Html { get; set; }

        [Display(Name = "Text Comment")]
        public string Text { get; set; }

        public int AuthorId { get; set; }

        public DateTimeOffset Posted { get; set; }

        public virtual User Author { get; set; }
        public virtual IReadOnlyCollection<BatchComment> BatchComments { get; set; }
        public virtual IReadOnlyCollection<Org.OrganizationComment> OrganizationComments { get; set; }
        public virtual IReadOnlyCollection<StatementComment> StatementComments { get; set; }
        public virtual IReadOnlyCollection<ReturnComment> ReturnComments { get; set; }
        public virtual IReadOnlyCollection<UserMention> UserMentions { get; set; }
    }
}
