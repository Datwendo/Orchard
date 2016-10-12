using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Settings;
using Orchard.Users.Models;
using Orchard.Security;
using System.Xml.Linq;
using Orchard.Services;
using System.Globalization;
using System.Text;
using Orchard.Messaging.Services;
using Orchard.Environment.Configuration;
using Orchard.Users.Events;
using Orchard.Users.Constants;
using System.Text.RegularExpressions;

namespace Orchard.Users.Services {
    public class UserService : IUserService {
        private static readonly TimeSpan DelayToValidate = new TimeSpan(7, 0, 0, 0); // one week to validate email
        private static readonly TimeSpan DelayToResetPassword = new TimeSpan(1, 0, 0, 0); // 24 hours to reset password

        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IMembershipService> _membershipServices;
        private readonly IClock _clock;
        private readonly IMessageService _messageService;
        private readonly IEncryptionService _encryptionService;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly ISiteService _siteService;
        private readonly IUserEventHandler _userEventHandlers;

        public UserService(
            IContentManager contentManager, 
            IEnumerable<IMembershipService> membershipServices, 
            IClock clock, 
            IMessageService messageService, 
            ShellSettings shellSettings, 
            IEncryptionService encryptionService,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay,
            IUserEventHandler userEventHandlers,
            ISiteService siteService
            ) {
            _userEventHandlers = userEventHandlers;
            _contentManager = contentManager;
            _membershipServices = membershipServices;
            _clock = clock;
            _messageService = messageService;
            _encryptionService = encryptionService;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;
            _siteService = siteService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        // CS 17/7
        public bool DeleteUser(int id) {
            var userPart = _contentManager.Get<IUser>(id);

            var userParameters = new RemoveUserParams(userPart,userPart.UserName, userPart.Email);
            RemoveUserContext urc = new RemoveUserContext { Cancel = false, User = userPart, UserParameters = userParameters };
            _userEventHandlers.Removing(urc);
            if (urc.Cancel) {
                Logger.Debug("Can't delete User '{0}' with email'{1}'", userPart.UserName, userPart.Email);
                return false;
            }
            _contentManager.Remove(userPart.ContentItem);
            _userEventHandlers.Removed(urc);
            Logger.Debug("User '{0}' Deleted", userPart.UserName);
            return true;
        }


        public bool VerifyUserUnicity(string userName, string email) {
            string normalizedUserName = userName.ToLowerInvariant();

            if (_contentManager.Query<UserPart, UserPartRecord>()
                                   .Where(user => 
                                          user.NormalizedUserName == normalizedUserName || 
                                          user.Email == email)
                                   .List().Any()) {
                return false;
            }

            return true;
        }

        public bool VerifyUserUnicity(int id, string userName, string email) {
            string normalizedUserName = userName.ToLowerInvariant();

            if (_contentManager.Query<UserPart, UserPartRecord>()
                                   .Where(user =>
                                          user.NormalizedUserName == normalizedUserName ||
                                          user.Email == email)
                                   .List().Any(user => user.Id != id)) {
                return false;
            }

            return true;
        }

        public string CreateNonce(IUser user, TimeSpan delay) {
            var challengeToken = new XElement("n", new XAttribute("un", user.UserName), new XAttribute("utc", _clock.UtcNow.ToUniversalTime().Add(delay).ToString(CultureInfo.InvariantCulture))).ToString();
            var data = Encoding.UTF8.GetBytes(challengeToken);
            return Convert.ToBase64String(_encryptionService.Encode(data));
        }

        public bool DecryptNonce(string nonce, out string username, out DateTime validateByUtc) {
            username = null;
            validateByUtc = _clock.UtcNow;

            try {
                var data = _encryptionService.Decode(Convert.FromBase64String(nonce));
                var xml = Encoding.UTF8.GetString(data);
                var element = XElement.Parse(xml);
                username = element.Attribute("un").Value;
                validateByUtc = DateTime.Parse(element.Attribute("utc").Value, CultureInfo.InvariantCulture);
                return _clock.UtcNow <= validateByUtc;
            }
            catch {
                return false;
            }

        }

        public IUser ValidateChallenge(string nonce) {
            string username;
            DateTime validateByUtc;

            if (!DecryptNonce(nonce, out username, out validateByUtc)) {
                return null;
            }

            if (validateByUtc < _clock.UtcNow)
                return null;

            IUser user = null;
            foreach (var membershipService in _membershipServices) {
                user = membershipService.GetUser(username,false);
                if (user != null)
                    break;
            }

            if (user == null)
                return null;

            user.As<UserPart>().EmailStatus = UserStatus.Approved;

            return user;
        }

        public void SendChallengeEmail(IUser user, Func<string, string> createUrl) {
            string nonce = CreateNonce(user, DelayToValidate);
            string url = createUrl(nonce);

            if (user != null) {
                var site = _siteService.GetSiteSettings();

                var template = _shapeFactory.Create("Template_User_Validated", Arguments.From(new {
                    RegisteredWebsite = site.As<RegistrationSettingsPart>().ValidateEmailRegisteredWebsite,
                    ContactEmail = site.As<RegistrationSettingsPart>().ValidateEmailContactEMail,
                    ChallengeUrl = url
                }));
                template.Metadata.Wrappers.Add("Template_User_Wrapper");
                
                var parameters = new Dictionary<string, object> {
                            {"Subject", T("Verification E-Mail").Text},
                            {"Body", _shapeDisplay.Display(template)},
                            {"Recipients", user.Email}
                        };

                _messageService.Send("Email", parameters);
            }
        }

        public bool SendLostPasswordEmail(string usernameOrEmail, Func<string, string> createUrl) {
            var lowerName = usernameOrEmail.ToLowerInvariant();
            var user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName || u.Email == lowerName).List().FirstOrDefault();

            if (user != null) {
                string nonce = CreateNonce(user, DelayToResetPassword);
                string url = createUrl(nonce);

                var template = _shapeFactory.Create("Template_User_LostPassword", Arguments.From(new {
                    User = user,
                    LostPasswordUrl = url
                }));
                template.Metadata.Wrappers.Add("Template_User_Wrapper");

                var parameters = new Dictionary<string, object> {
                            {"Subject", T("Lost password").Text},
                            {"Body", _shapeDisplay.Display(template)},
                            {"Recipients", user.Email }
                        };

                _messageService.Send("Email", parameters);
                return true;
            }

            return false;
        }

        public IUser ValidateLostPassword(string nonce) {
            string username;
            DateTime validateByUtc;

            if (!DecryptNonce(nonce, out username, out validateByUtc)) {
                return null;
            }

            if (validateByUtc < _clock.UtcNow)
                return null;

            IUser user = null;
            foreach (var membershipService in _membershipServices) {
                user = membershipService.GetUser(username,false);
                if (user != null)
                    break;
            }
            if (user == null)
                return null;

            return user;
        }

        public bool PasswordMeetsPolicies(string password, out IDictionary<string, LocalizedString> validationErrors) {
            validationErrors = new Dictionary<string, LocalizedString>();
            var settings = _siteService.GetSiteSettings().As<RegistrationSettingsPart>();

            if(string.IsNullOrEmpty(password)) {
                validationErrors.Add(UserPasswordValidationResults.PasswordIsTooShort,
                    T("The password can't be empty."));
                return false;
            }

            if(password.Length < settings.GetMinimumPasswordLength()) {
                validationErrors.Add(UserPasswordValidationResults.PasswordIsTooShort,
                    T("You must specify a password of {0} or more characters.", settings.MinimumPasswordLength));
            }

            if(settings.EnableCustomPasswordPolicy) {
                if(settings.EnablePasswordNumberRequirement && !Regex.Match(password, "[0-9]").Success) {
                    validationErrors.Add(UserPasswordValidationResults.PasswordDoesNotContainNumbers,
                        T("The password must contain at least one number."));
                }
                if(settings.EnablePasswordUppercaseRequirement && !password.Any(c => char.IsUpper(c))) {
                    validationErrors.Add(UserPasswordValidationResults.PasswordDoesNotContainUppercase,
                        T("The password must contain at least one uppercase letter."));
                }
                if(settings.EnablePasswordLowercaseRequirement && !password.Any(c => char.IsLower(c))) {
                    validationErrors.Add(UserPasswordValidationResults.PasswordDoesNotContainLowercase,
                        T("The password must contain at least one lowercase letter."));
                }
                if(settings.EnablePasswordSpecialRequirement && !Regex.Match(password, "[^a-zA-Z0-9]").Success) {
                    validationErrors.Add(UserPasswordValidationResults.PasswordDoesNotContainSpecialCharacters,
                        T("The password must contain at least one special character."));
                }
            }

            return validationErrors.Count == 0;
        }
    }
}