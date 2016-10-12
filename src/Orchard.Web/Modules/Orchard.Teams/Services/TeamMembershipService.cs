using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Teams.Services {
    public class TeamMembershipService : IMembershipService {
        private readonly ITeamService _teamService;
        private readonly IOrchardServices _orchardServices;

        public TeamMembershipService(IOrchardServices orchardServices, ITeamService teamService) {
            _orchardServices = orchardServices;
            _teamService = teamService;
        }

        public bool IsMain { get { return false; } }
        public IMembershipSettings GetSettings() {
            return _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
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
        public bool PasswordIsExpired(IUser user, int days) {
            return false;
        }
        public void SetPassword(IUser user, string password) {

        }

    }
}