using System;
using Orchard.ContentManagement;
using Orchard.Security;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Teams.Models {
    public sealed class TeamPart : ContentPart<TeamPartRecord>, IUser {

        public string TeamName {
            get { return Retrieve(x => x.TeamName); }
            set { Store(x => x.TeamName, value); }
        }
        public string NormalizedTeamName {
            get { return Retrieve(x => x.NormalizedTeamName); }
            set { Store(x => x.NormalizedTeamName, value); }
        }

        public string Email {
            get { return Retrieve(x => x.Email); }
            set { Store(x => x.Email, value); }
        }

        public IEnumerable<Tuple<TeamMemberType, int>> Members {
            get {
                var teamUser = this.ContentItem.As<TeamMembersPart>();
                if(teamUser != null)
                    return teamUser.Members;
                return Enumerable.Empty<Tuple<TeamMemberType, int>>();
            }
        }

        public IList<int> Teams {
            get {
                var teamsofThisTeam = this.ContentItem.As<UserTeamsPart>();
                return teamsofThisTeam?.Teams;
            }
        }
        public TeamMemberType TeamMemberType { get { return TeamMemberType.team; } }

        public string UserName { get { return TeamName; } }

        public DateTime? CreatedUtc {
            get { return Retrieve(x => x.CreatedUtc); }
            set { Store(x => x.CreatedUtc, value); }
        }

        public DateTime? LastLoginUtc {
            get { return Retrieve(x => x.LastLoginUtc); }
            set { Store(x => x.LastLoginUtc, value); }
        }

        public DateTime? LastLogoutUtc {
            get { return Retrieve(x => x.LastLogoutUtc); }
            set { Store(x => x.LastLogoutUtc, value); }
        }

    }
}
