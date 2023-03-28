using Cohub.Data.Geo;
using Microsoft.EntityFrameworkCore;
using SiteKit.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace Cohub.Data.Org
{
    public class Organization
    {
        private DbContext Db { get; set; }

        public Organization()
        {
        }

        public Organization(DateTimeOffset created)
        {
            Created = created;
        }

        public Organization(Organization input)
        {
            Id = input.Id;
            UpdateWith(input);
        }

        public void UpdateWith(Organization input)
        {
            StatusId = input.StatusId;
            OrganizationName = input.OrganizationName;
            Dba = input.Dba;
            OnlineFiler = input.OnlineFiler;
            SendPhysicalMail = input.SendPhysicalMail;
            OrganizationDescription = input.OrganizationDescription;
            Restrictions = input.Restrictions;
            ClassificationId = input.ClassificationId;
            TypeId = input.TypeId;
            OrganizationEmail = input.OrganizationEmail;
            OrganizationPhoneNumber = input.OrganizationPhoneNumber;
            PhysicalAddress.UpdateWith(input.PhysicalAddress);
            MailingAddress.UpdateWith(input.MailingAddress);
            StateID = input.StateID;
            FederalID = input.FederalID;
        }

        public override string ToString()
        {
            return $"{Id} {Dba.Nullify() ?? OrganizationName}";
        }

        [Required]
        [StringLength(25)]
        public string Id { get; set; }

        [DataType("OrganizationStatusId")]
        public string StatusId { get; set; } = OrganizationStatusId.Pending;

        [Required]
        public string OrganizationName { get; set; }

        [Display(Name = "DBA")]
        public string Dba { get; set; }

        public bool OnlineFiler { get; set; } = true;

        public bool SendPhysicalMail { get; set; } = false;

        [DataType(DataType.MultilineText)]
        public string OrganizationDescription { get; set; }

        [DataType(DataType.MultilineText)]
        public string Restrictions { get; set; }

        [Required]
        [DataType("OrganizationClassificationId")]
        public string ClassificationId { get; set; } = OrganizationClassificationId.Unclassified;

        [Required]
        [DataType("OrganizationTypeId")]
        public string TypeId { get; set; } = OrganizationTypeId.Unknown;

        [EmailAddress]
        public string OrganizationEmail { get; set; }

        [Phone]
        public string OrganizationPhoneNumber { get; set; }

        public Address PhysicalAddress { get; set; } = new Address();

        public Address MailingAddress { get; set; } = new Address();

        [ScaffoldColumn(false)]
        public string StateID { get; set; }

        [ScaffoldColumn(false)]
        public string FederalID { get; set; }

        public DateTimeOffset Created { get; private set; }
        public int CreatedById { get; private set; }

        public DateTimeOffset Updated { get; private set; }
        public int UpdatedById { get; private set; }

        public virtual OrganizationClassification Classification { get; private set; }
        public virtual OrganizationStatus Status { get; private set; }
        public virtual OrganizationType Type { get; private set; }
        public virtual Usr.User CreatedBy { get; private set; }
        public virtual Usr.User UpdatedBy { get; private set; }

        [ScaffoldColumn(false)]
        [DataType("LabelList")]
        public virtual List<Label> Labels { get; set; }

        [ScaffoldColumn(false)]
        [DataType("OrganizationContactList")]
        public virtual List<OrganizationContact> Contacts { get; set; }

        [DataType("LicenseList")]
        public virtual IReadOnlyList<Lic.License> Licenses { get; private set; }

        [DataType("OrganizationCommentList")]
        public virtual IReadOnlyList<OrganizationComment> Comments { get; private set; }

        public virtual IReadOnlyList<Fin.FilingSchedule> FilingSchedules { get; private set; }
        public virtual IReadOnlyList<Fin.Return> Returns { get; private set; }
        public virtual IReadOnlyList<Fin.Statement> Statements { get; private set; }
        public virtual IReadOnlyList<Fin.TransactionDetail> TransactionDetails { get; private set; }

        /// <summary>
        /// Loads all collections except <see cref="Comments"/>.
        /// </summary>
        /// <returns></returns>
        public async Task LoadCollectionsAsync()
        {
            var entry = Db.Entry(this);

            await entry.Collection(o => o.Labels).LoadAsync();

            await entry.Collection(o => o.FilingSchedules).LoadAsync();
            foreach (var item in FilingSchedules) await Db.Entry(item).Reference(o => o.PaymentChart).LoadAsync();

            await entry.Collection(o => o.Licenses).LoadAsync();
            foreach (var item in Licenses) await Db.Entry(item).Reference(o => o.Type).LoadAsync();

            await entry.Collection(o => o.Contacts).LoadAsync();
            foreach (var item in Contacts) await Db.Entry(item).Reference(o => o.Relationship).LoadAsync();
        }

        /// <summary>
        /// Loads <see cref="Comments"/> whose <see cref="Cohub.Data.Usr.Comment.AuthorId"/> is not System or Anonymous
        /// ordered by <see cref="Usr.Comment.Posted"/>.
        /// </summary>
        /// <returns></returns>
        public async Task LoadUserCommentsAsync()
        {
            if (Comments is null && !Id.IsNullOrWhiteSpace())
            {
                Comments = await Db.Set<OrganizationComment>()
                    .Include(o => o.Comment).ThenInclude(o => o.Author)
                    .Include(o => o.Comment).ThenInclude(o => o.BatchComments).ThenInclude(o => o.Batch)
                    .Include(o => o.Comment).ThenInclude(o => o.OrganizationComments).ThenInclude(o => o.Organization)
                    .Include(o => o.Comment).ThenInclude(o => o.StatementComments).ThenInclude(o => o.Statement)
                    .Include(o => o.Comment).ThenInclude(o => o.ReturnComments).ThenInclude(o => o.Return)
                    .Include(o => o.Comment).ThenInclude(o => o.UserMentions).ThenInclude(o => o.User)
                    .AsSplitQuery()
                    .Where(o =>
                        o.OrganizationId == Id &&
                        o.Comment.IsUserComment
                    )
                    .OrderByDescending(o => o.Id)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Loads <paramref name="take"/> <see cref="Comments"/> ordered by <see cref="Usr.Comment.Posted"/>.
        /// </summary>
        /// <returns></returns>
        public async Task LoadCommentsAsync()
        {
            if (Comments is null && !Id.IsNullOrWhiteSpace())
            {
                Comments = await Db.Set<OrganizationComment>()
                    .Include(o => o.Comment).ThenInclude(o => o.Author)
                    .Include(o => o.Comment).ThenInclude(o => o.BatchComments).ThenInclude(o => o.Batch)
                    .Include(o => o.Comment).ThenInclude(o => o.OrganizationComments).ThenInclude(o => o.Organization)
                    .Include(o => o.Comment).ThenInclude(o => o.StatementComments).ThenInclude(o => o.Statement)
                    .Include(o => o.Comment).ThenInclude(o => o.ReturnComments).ThenInclude(o => o.Return)
                    .Include(o => o.Comment).ThenInclude(o => o.UserMentions).ThenInclude(o => o.User)
                    .AsSplitQuery()
                    .Where(o => o.OrganizationId == Id)
                    .OrderByDescending(o => o.Id)
                    .ToListAsync();
            }
        }
    }
}
