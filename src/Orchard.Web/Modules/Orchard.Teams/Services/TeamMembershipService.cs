using Orchard.Security;

namespace Orchard.Teams.Services {
    public class TeamMembershipService : IMembershipService {
        private readonly ITeamService _teamService;

        public TeamMembershipService(ITeamService teamService) {
            _teamService = teamService;
        }

        public bool IsMain { get { return false; } }
        public MembershipSettings GetSettings() {
            var settings = new MembershipSettings();
            // accepting defaults
            return settings;

        }

        public IUser CreateUser(CreateUserParams createUserParams) {
            return _teamService.CreateTeam(createUserParams.Username, createUserParams.Email, createUserParams.FirstUser);

        }
        public IUser GetUser(string username) {
            return _teamService.GetTeamByName(username);
        }
        public IUser GetUser(string username, bool isTeam = false) {
            if ( isTeam )
                return _teamService.GetTeamByName(username);
            return null;
        }

        public IUser ValidateUser(string userNameOrEmail, string password) {
            return null;
        }
        public void SetPassword(IUser user, string password) {

        }

    }
}