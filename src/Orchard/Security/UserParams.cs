namespace Orchard.Security {
    // CS 17/7
    public class UserParams {

        public UserParams(string username, string email) {
            Username = username;
            Email = email;
        }

        public string Username { get; set;}

        public string Email { get; set; }

    }
}