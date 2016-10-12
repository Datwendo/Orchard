using System.Collections.Generic;
using Orchard.Teams.Models;
using Orchard.Security;

namespace Orchard.Teams.ViewModels {
    public class TeamMembersViewModel {
        public TeamMembersViewModel() {
            Members = new List<TeamMemberEntry>();
        }

        public TeamMembersPart Team { get; set; }
        public IList<TeamMemberEntry> Members { get; set; }
    }

    public class TeamMemberEntry {
        public int Id { get; set; }
        public string Name { get; set; }
        public TeamMemberType TeamMemberType { get; set; }
        public bool Checked { get; set; }
    }
}
