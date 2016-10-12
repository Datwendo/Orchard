using Orchard.Security;

namespace Orchard.Teams.Events {
    public class UserRemoveContext : TeamUsersContext {
        public IUser User { get; set; }
        public bool Cancel { get; set; }
    }
}