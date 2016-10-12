using System;

namespace Orchard.Security {
    public interface IMembershipService : IDependency {
        MembershipSettings GetSettings();
        bool IsMain { get; }
        IUser CreateUser(CreateUserParams createUserParams);
        IUser GetUser(string username);
        IUser GetUser(string name, bool isTeam);
        IUser ValidateUser(string userNameOrEmail, string password);
        void SetPassword(IUser user, string password);
    }
}
