using Orchard.Security;

namespace Orchard.Teams.Events {
    public class TeamCreateContext : TeamContext {
        public CreateTeamParams TeamParameters { get; set; }
    }
    public class CreateTeamParams : UserParams {
        public IUser FirstUser { get; set; }
        public CreateTeamParams(string username, string email,IUser firstUser)
            : base(username, email) {
            FirstUser = firstUser;
        }
    }
}