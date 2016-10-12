using System.Collections.Generic;
using Orchard.ContentManagement;
using System;

namespace Orchard.Security {
    public enum TeamMemberType { user=0,team=1};
    public interface ITeamMembers : IContent {
        IList<Tuple<TeamMemberType,int>>Members { get; }
    }
}