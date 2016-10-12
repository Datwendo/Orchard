using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using Orchard.Security;

namespace Orchard.Teams.Models {
    public class UserTeamsPart : ContentPart, ITeams {

        internal LazyField<IList<int>> _teams = new LazyField<IList<int>>();

        public IList<int> Teams {
            get { return _teams.Value; }
        }
    }
}