using System.Linq;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Teams.Models;

namespace Orchard.Teams.Handlers {
    public class UserTeamsPartHandler : ContentHandler {
        private readonly IRepository<TeamMembersPartRecord> _teamUsersRepository;

        public UserTeamsPartHandler(IRepository<TeamMembersPartRecord> teamUsersRepository) {
            _teamUsersRepository = teamUsersRepository;

            Filters.Add (new ActivatingFilter<UserTeamsPart> ("User"));
            Filters.Add (new ActivatingFilter<UserTeamsPart> ("Team"));
            OnInitialized<UserTeamsPart>((context, userTeams) => userTeams._teams.Loader(() => _teamUsersRepository
                .Fetch(x => x.UserId == context.ContentItem.Id)
                .Select(x => x.TeamId).ToList()));
        }
    }
}