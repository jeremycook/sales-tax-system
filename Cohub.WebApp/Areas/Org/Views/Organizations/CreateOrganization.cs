using Cohub.Data.Fin;
using Cohub.Data.Geo;
using Cohub.Data.Org;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace Cohub.WebApp.Areas.Org.Views.Organizations
{
    public class CreateOrganization
    {
        [Required]
        [StringLength(25)]
        public string Id { get; set; }

        [Required]
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
        public string ClassificationId { get; set; }

        [Required]
        [DataType("OrganizationTypeId")]
        public string TypeId { get; set; }

        [DataType("IndustryLabelId")]
        public string IndustryLabelId { get; set; }

        [DataType("GeocodeLabelId")]
        public string GeocodeLabelId { get; set; }

        public string StateID { get; set; }

        public string FederalID { get; set; }

        [EmailAddress]
        public string OrganizationEmail { get; set; }

        [Phone]
        public string OrganizationPhoneNumber { get; set; }

        [Required]
        public Address PhysicalAddress { get; set; } = new Address();

        [Required]
        public Address MailingAddress { get; set; } = new Address();

        public CreateOrganizationFilingSchedule FilingSchedule { get; set; } = new();

        public class CreateOrganizationFilingSchedule
        {
            [DataType("PaymentChartId")]
            public int? PaymentChartId { get; set; }

            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; } = DateTime.Today;
        }

        public async Task<Organization> AddOrganizationAsync(Data.CohubDbContext db)
        {
            Organization organization = new()
            {
                Id = this.Id,
                StatusId = this.StatusId,
                OrganizationName = this.OrganizationName,
                Dba = this.Dba,
                OnlineFiler = this.OnlineFiler,
                SendPhysicalMail = this.SendPhysicalMail,
                OrganizationDescription = this.OrganizationDescription,
                Restrictions = this.Restrictions,
                ClassificationId = this.ClassificationId,
                TypeId = this.TypeId,
                StateID = this.StateID,
                FederalID = this.FederalID,
                OrganizationEmail = this.OrganizationEmail,
                OrganizationPhoneNumber = this.OrganizationPhoneNumber,
                PhysicalAddress = this.PhysicalAddress,
                MailingAddress = this.MailingAddress,
                Labels = new(),
            };
            db.Add(organization);

            if (!this.IndustryLabelId.IsNullOrWhiteSpace())
            {
                organization.Labels.Add(await db.Labels().FindAsync(this.IndustryLabelId));
            }

            if (!this.GeocodeLabelId.IsNullOrWhiteSpace())
            {
                organization.Labels.Add(await db.Labels().FindAsync(this.GeocodeLabelId));
            }

            if (this.FilingSchedule.PaymentChartId.HasValue)
            {
                db.Add(new FilingSchedule
                {
                    OrganizationId = organization.Id,
                    PaymentChartId = await db.PaymentCharts()
                        .Where(o => o.Id == FilingSchedule.PaymentChartId.Value)
                        .Select(o => o.Id)
                        .SingleAsync(),
                    StartDate = FilingSchedule.StartDate,
                    EndDate = DateTime.MaxValue.Date,
                });
            }

            db.Comment($"Created organization {organization}.", new OrganizationComment(organization));

            return organization;
        }
    }
}
