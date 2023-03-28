using Cohub.Data.Usr;
using Microsoft.EntityFrameworkCore;
using SiteKit.EntityFrameworkCore;
using SiteKit.Info;
using SiteKit.Text;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cohub.Data
{
    public abstract class CohubDbContextBase : DbContext
    {
        public static readonly SortedSet<string> CreatedPropertyNames = new SortedSet<string> { "Created", "Inserted" };
        public static readonly SortedSet<string> CreatedByIdPropertyNames = new SortedSet<string> { "CreatedById", "InsertedById" };
        public static readonly SortedSet<string> UpdatedPropertyNames = new SortedSet<string> { "Updated", "Modified" };
        public static readonly SortedSet<string> UpdatedByIdPropertyNames = new SortedSet<string> { "UpdatedById", "ModifiedById" };

        [Obsolete("Design-time")]
        public CohubDbContextBase()
        {
            Actor = Actor.System;
        }

        public CohubDbContextBase(DbContextOptions options, Actor actor)
            : base(options)
        {
            Actor = actor;
        }

        public Actor Actor { get; }

        /// <summary>
        /// Adds a user <see cref="Comment"/> without saving the context.
        /// </summary>
        /// <param name="commentText"></param>
        /// <param name="commentReferences"></param>
        public Comment UserComment(string commentText, params ICommentReference[] commentReferences)
        {
            return Comment(commentText: commentText, isUserComment: true, commentReferences: commentReferences); ;
        }

        /// <summary>
        /// Adds a non-user <see cref="Comment"/> without saving the context.
        /// </summary>
        /// <param name="commentText"></param>
        /// <param name="commentReferences"></param>
        public Comment Comment(string commentText, params ICommentReference[] commentReferences)
        {
            return Comment(commentText: commentText, isUserComment: false, commentReferences: commentReferences); ;
        }

        /// <summary>
        /// Adds a <see cref="Comment"/> without saving the context.
        /// </summary>
        /// <param name="commentText"></param>
        /// <param name="commentReferences"></param>
        public Comment Comment(string commentText, bool isUserComment, params ICommentReference[] commentReferences)
        {
            var comment = new Comment
            {
                AuthorId = Actor.UserId,
                Html = Html.Interpolate($"<p>{commentText}</p>").ToString(),
                Text = commentText,
                Posted = Actor.Now,
                IsUserComment = isUserComment,
            };
            foreach (var reference in commentReferences)
            {
                reference.ReferenceComment(comment);
                Add(reference);
            }
            Add(comment);
            return comment;
        }

        public void ReferenceComment(Comment comment, params ICommentReference[] commentReferences)
        {
            foreach (var reference in commentReferences)
            {
                reference.ReferenceComment(comment);
                Add(reference);
            }
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            TransformChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            TransformChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void TransformChanges()
        {
            foreach (var change in ChangeTracker.Entries())
            {
                if (change.State == EntityState.Added)
                {
                    foreach (var prop in change.Properties)
                    {
                        if (prop.CurrentValue is string val)
                        {
                            prop.CurrentValue = val.Trim();
                        }
                        else if (prop.CurrentValue is DateTimeOffset dto)
                        {
                            if (CreatedPropertyNames.Contains(prop.Metadata.Name) &&
                                dto == DateTimeOffset.MinValue)
                                prop.CurrentValue = Actor.Now;
                            else if (UpdatedPropertyNames.Contains(prop.Metadata.Name))
                                prop.CurrentValue = Actor.Now;
                        }
                        else if (prop.CurrentValue is int userId)
                        {
                            if (CreatedByIdPropertyNames.Contains(prop.Metadata.Name) &&
                                userId == 0)
                                prop.CurrentValue = Actor.UserId;
                            else if (UpdatedByIdPropertyNames.Contains(prop.Metadata.Name))
                                prop.CurrentValue = Actor.UserId;
                        }
                    }
                }
                else if (change.State == EntityState.Modified)
                {
                    foreach (var prop in change.Properties)
                    {
                        if (prop.CurrentValue is string val)
                        {
                            prop.CurrentValue = val.Trim();
                        }
                        if (prop.CurrentValue is DateTimeOffset)
                        {
                            if (UpdatedPropertyNames.Contains(prop.Metadata.Name))
                                prop.CurrentValue = Actor.Now;
                        }
                        else if (prop.CurrentValue is int)
                        {
                            if (UpdatedByIdPropertyNames.Contains(prop.Metadata.Name))
                                prop.CurrentValue = Actor.UserId;
                        }
                    }
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("sts");
            modelBuilder.ApplyModelBuilderContributors(typeof(CohubDbContextBase).Assembly);
            modelBuilder.ApplySingularTableNames();
            modelBuilder.ApplyUnderscoreNames(force: true);
            modelBuilder.ApplyDateColumnType();
        }
    }
}
