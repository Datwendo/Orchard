using System.Collections.Generic;
using Orchard.Teams.Models;

namespace Orchard.Teams.ViewModels {
    public class UserTeamsViewModel {
        public UserTeamsViewModel() {
            Teams = new List<UserTeamEntry>();
        }

        public UserTeamsPart User { get; set; }
        public IList<UserTeamEntry> Teams { get; set; }
    }

    public class UserTeamEntry {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
    }
}
