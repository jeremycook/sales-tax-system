using Cohub.Data.Usr;
using SiteKit.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Usr.Views.Users
{
    public class UserInputModel
    {
        public UserInputModel()
        {
            IsActive = true;
        }
        public UserInputModel(User user)
        {
            IsActive = user.IsActive;
            Username = user.Username;
            Name = user.Name;
            Initials = user.Initials;
            Email = user.Email;
            RoleId = user.RoleId;
        }

        [Boolean("Active", "Disabled")]
        public bool IsActive { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Initials { get; set; }

        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        [DataType("RoleId")]
        [Display(Name = "Role")]
        public string? RoleId { get; set; }

        /// <summary>
        /// Creates and adds the <paramref name="user"/>, and saves changes.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<User> AddUserAsync(Data.CohubDbContext db)
        {
            User user = new()
            {
                IsActive = IsActive,
                Username = Username ?? throw new ValidationException("The Username field is required."),
                Name = Name ?? throw new ValidationException("The Name field is required."),
                Initials = Initials,
                Email = Email,
                RoleId = RoleId ?? throw new ValidationException("The Role field is required."),
            };
            db.Add(user);

            db.Comment($"Created user {user}.", new UserMention(user));

            await db.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Updates the <paramref name="user"/>, and saves changes.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task UpdateAsync(User user, Data.CohubDbContext db)
        {
            user.IsActive = IsActive;
            user.Username = Username ?? throw new ValidationException("The Username field is required.");
            user.Name = Name ?? throw new ValidationException("The Name field is required.");
            user.Initials = Initials;
            user.Email = Email;
            user.RoleId = RoleId ?? throw new ValidationException("The Role field is required.");

            await db.SaveChangesAsync();
        }
    }
}
