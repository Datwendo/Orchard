using System.Collections.Generic;
using Orchard.Security;
using Orchard.Teams.Models;

namespace Orchard.Teams.ViewModels {

    public class TeamsIndexViewModel  {
        public IList<TeamEntry> Teams { get; set; }
        public TeamIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class TeamEntry {
        public TeamPartRecord Team { get; set; }
        public IEnumerable<string> Teams { get; set; }
        public bool IsChecked { get; set; }
    }

    public class TeamIndexOptions {
        public string Search { get; set; }
        public TeamsOrder Order { get; set; }
        public TeamsBulkAction BulkAction { get; set; }
    }

    public enum TeamsOrder {
        Name,
        Email,
        CreatedUtc,
        LastLoginUtc
    }


    public enum TeamsBulkAction {
        None,
        Delete
    }
}
