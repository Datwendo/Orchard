using System.ComponentModel.DataAnnotations;

namespace Orchard.Teams.ViewModels {
    public class TeamCreateViewModel  {
        [Required]
        public string TeamName { get; set; }

        [Required]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}