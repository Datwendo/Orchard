namespace Orchard.Security {
    // // CS 17/7
    public class CreateUserParams : UserParams {
        
        public CreateUserParams(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, string firstUser = null)
            : base(username, email){
            Password = password;
            PasswordQuestion = passwordQuestion;
            PasswordAnswer = passwordAnswer;
            IsApproved = isApproved;
            FirstUser = firstUser;
        }

        public string Password {get; set;}
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
        public string FirstUser { get; set; }
    }
}