using Orchard.Security;
using System;
using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Users.Services {
    public interface IUserService : IDependency {
        // CS 17/7
        bool DeleteUser(int id);
        bool VerifyUserUnicity(string userName, string email);
        bool VerifyUserUnicity(int id, string userName, string email);

        void SendChallengeEmail(IUser user, Func<string, string> createUrl);
        IUser ValidateChallenge(string challengeToken);

        bool SendLostPasswordEmail(string usernameOrEmail, Func<string, string> createUrl);
        IUser ValidateLostPassword(string nonce);

        string CreateNonce(IUser user, TimeSpan delay);
        bool DecryptNonce(string challengeToken, out string username, out DateTime validateByUtc);

        bool PasswordMeetsPolicies(string password, out IDictionary<string, LocalizedString> validationErrors);

    }
}