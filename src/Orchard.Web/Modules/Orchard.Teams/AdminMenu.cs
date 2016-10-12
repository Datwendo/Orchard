using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.Users;

namespace Orchard.Teams {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("teams")
                .Add(T("Teams"), "12",
                    menu => menu.Action("Index", "Admin", new { area = "Orchard.Teams" })
                        .Add(T("Teams"), "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Teams" })
                            .LocalNav().Permission(Permissions.ManageUsers)));
        }
    }
}
