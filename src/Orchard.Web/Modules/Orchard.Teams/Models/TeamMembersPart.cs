using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System;
using Orchard.Security;

namespace Orchard.Teams.Models {
    public class TeamMembersPart : ContentPart, ITeamMembers {

        internal LazyField<IList<Tuple<TeamMemberType, int>>> _members = new LazyField<IList<Tuple<TeamMemberType, int>>>();

        public IList<Tuple<TeamMemberType,int>> Members {
            get { return _members.Value; }
        }
    }
}