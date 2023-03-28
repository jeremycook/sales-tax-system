using System;

#nullable disable

namespace Cohub.Data.Org
{
    public class OrganizationComment : Usr.ICommentReference
    {
        public OrganizationComment() { }
        public OrganizationComment(Organization organization)
        {
            Organization = organization;
        }
        public OrganizationComment(string organizationId)
        {
            OrganizationId = organizationId;
        }

        public int Id { get; private set; }
        public string OrganizationId { get; private set; }
        public int CommentId { get; private set; }
        public DateTimeOffset Created { get; private set; }

        public virtual Organization Organization { get; private set; }
        public virtual Usr.Comment Comment { get; private set; }

        void Usr.ICommentReference.ReferenceComment(Usr.Comment comment)
        {
            Comment = comment;
        }

        public override string ToString()
        {
            return Organization?.ToString() ?? $"Organization:{OrganizationId}";
        }
    }
}
