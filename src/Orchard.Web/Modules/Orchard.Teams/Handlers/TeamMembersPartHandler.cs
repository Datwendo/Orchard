using System.Linq;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Teams.Models;
using Orchard.Security;
using System;

namespace Orchard.Teams.Handlers {
    public class TeamMembersPartHandler : ContentHandler {
        private readonly IRepository<TeamMembersPartRecord> _teamMembersRepository;

        public TeamMembersPartHandler(IRepository<TeamMembersPartRecord> teamMembersRepository) {
            _teamMembersRepository = teamMembersRepository;

            Filters.Add(new ActivatingFilter<TeamMembersPart>("Team"));
            
            OnInitialized<TeamMembersPart>((context, teamMembers) => teamMembers._members.Loader(() => _teamMembersRepository
                .Fetch(x => x.TeamId == context.ContentItem.Id)
                .Select(x => Tuple.Create(TeamMemberType.user,x.UserId)).ToList()));
        }
    }
}