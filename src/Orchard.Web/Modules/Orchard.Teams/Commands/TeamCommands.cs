using Orchard.Commands;
using Orchard.Security;
using Orchard.Teams.Services;

namespace Orchard.Teams.Commands {
    public class UserCommands : DefaultOrchardCommandHandler {
        private readonly IMembershipService _membershipService;
        private readonly ITeamService _teamService;

        public UserCommands(
            IMembershipService membershipService,
            ITeamService teamService) {
            _teamService = teamService;
            _membershipService = membershipService;
        }

        [OrchardSwitch]
        public string TeamName { get; set; }

        [OrchardSwitch]
        public string Email { get; set; }

        [OrchardSwitch]
        public string UserName { get; set; }

        [CommandName("team create")]
        [CommandHelp("Team create /TeamName:<teamname> /Email:<email> /UserName:<username>\r\n\t" + "Creates a new Team")]
        [OrchardSwitches("TeamName,Email,UserName")]
        public void Create() {
            if (string.IsNullOrWhiteSpace(TeamName)) {
                Context.Output.WriteLine(T("Team name cannot be empty."));
                return;
            }

            if (string.IsNullOrWhiteSpace(UserName)) {
                Context.Output.WriteLine(T("User name cannot be empty."));
                return;
            }

            if (!_teamService.VerifyTeamUnicity(TeamName, Email)) {
                Context.Output.WriteLine(T("Team with that name and/or email already exists."));
                return;
            }

            var team = _teamService.CreateTeam(TeamName, Email,UserName);
            if (team == null) {
                Context.Output.WriteLine(T("Could not create team {0}. The authentication provider returned an error", TeamName));
                return;
            }

            Context.Output.WriteLine(T("Team created successfully"));
        }
    }
}