using Orchard.Security;

namespace Orchard.Users.Events {
    public class CreateUserContext : UserContext {
        public CreateUserParams UserParameters { get; set; }
    }
}