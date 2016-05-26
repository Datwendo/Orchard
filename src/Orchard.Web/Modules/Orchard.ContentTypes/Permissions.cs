using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Orchard.ContentTypes {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ViewContentTypes = new Permission { Name = "ViewContentTypes", Description = "View content types." };
        public static readonly Permission EditContentTypes = new Permission { Name = "EditContentTypes", Description = "Edit content types." };
        // CS 25/05 
        public static readonly Permission FrontEditContentTypes = new Permission { Name = "FrontEditContentTypes", Description = "Front Edit content types." };
        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new [] {
                ViewContentTypes,
                EditContentTypes,
                FrontEditContentTypes // CS 25/05 
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = GetPermissions()
                }
            };
        }
    }
}
