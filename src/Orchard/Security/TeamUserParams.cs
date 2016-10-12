namespace Orchard.Security {
    // CS 17/7
    public class TeamUserParams  : UserParams {

        public TeamUserParams(IUser team, string username, string email) : base(username, email) {
            Team = team;
        }

        public IUser Team { get; set; }
    }
}