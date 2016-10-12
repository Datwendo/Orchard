using Orchard.Security;

namespace Orchard.Users.Events {
    public class RemoveUserContext {
        public IUser User { get; set; }
        public bool Cancel { get; set; }
        public RemoveUserParams UserParameters { get; set; }
    }
}