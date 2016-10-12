using Orchard.Security;
using Orchard.Users.Events;

namespace Orchard.Teams.Events {
    public class UserTeamContext : UserContext {
        public IUser Team { get; set; }
    }
}