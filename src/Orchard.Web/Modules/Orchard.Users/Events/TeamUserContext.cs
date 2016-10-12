using Orchard.Security;

namespace Orchard.Users.Events {
    public class TeamUserContext : UserContext {
        public TeamUserParams UserParameters { get; set; }
    }
}