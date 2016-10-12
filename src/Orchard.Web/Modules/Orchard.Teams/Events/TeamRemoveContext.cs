using Orchard.Security;
using System.Collections.Generic;

namespace Orchard.Teams.Events {
    public class TeamRemoveContext : TeamContext {
        public RemoveTeamParams TeamParameters { get; set; }
    }
    public class RemoveTeamParams {
        public IEnumerable<IUser> Users { get; set; }
    }
}
