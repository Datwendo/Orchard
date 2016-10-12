using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Teams.Models;
using System.Collections;

namespace Orchard.Teams.ViewModels {
    public class TeamEditViewModel  {
        [Required]
        public string TeamName {
            get { return Team.As<TeamPart>().TeamName; }
            set { Team.As<TeamPart>().TeamName = value; }
        }

        [Required]
        public string Email {
            get { return Team.As<TeamPart>().Email; }
            set { Team.As<TeamPart>().Email = value; }
        }

        public IEnumerable Members {
            get { return Team.As<TeamPart>().Members; }
        }

        public IContent Team { get; set; }
    }
}