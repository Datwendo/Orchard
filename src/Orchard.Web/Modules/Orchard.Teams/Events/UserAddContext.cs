using Orchard.Security;

namespace Orchard.Teams.Events {
    public class UserAddContext : TeamUsersContext {
        public IUser User { get; set; }
        public int TeamUserId { get; set; }
        public bool Cancel { get; set; }
    }
}