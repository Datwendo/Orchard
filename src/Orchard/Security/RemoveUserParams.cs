namespace Orchard.Security {
    // CS 17/7
    public class RemoveUserParams  : UserParams {

        public RemoveUserParams(IUser user, string username, string email) : base(username, email) {
        }
    }
}