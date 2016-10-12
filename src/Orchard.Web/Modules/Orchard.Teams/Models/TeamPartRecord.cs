using System;
using Orchard.ContentManagement.Records;

namespace Orchard.Teams.Models {
    public class TeamPartRecord : ContentPartRecord {
        public virtual string TeamName { get; set; }
        public virtual string Email { get; set; }
        public virtual string NormalizedTeamName { get; set; }

        public virtual DateTime? CreatedUtc { get; set; }
        public virtual DateTime? LastLoginUtc { get; set; }
        public virtual DateTime? LastLogoutUtc { get; set; }
    }
}