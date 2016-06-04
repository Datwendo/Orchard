using System.Web.Routing;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Contents.Handlers {
    public class ContentsHandler : ContentHandlerBase {
        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            if (context.Metadata.CreateRouteValues == null) {
                context.Metadata.CreateRouteValues = new RouteValueDictionary {
                    {"Area", "Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Create"},
                    {"Id", context.ContentItem.ContentType}
                };
            }

            if (context.Metadata.EditorRouteValues == null) {
                context.Metadata.EditorRouteValues = new RouteValueDictionary {
                    {"Area", "Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Edit"},
                    {"Id", context.ContentItem.Id}
                };
            }
            /* Include this in your contentItem handlers for FrontCretion/Edition
            if (context.Metadata.FrontCreateRouteValues == null) {
                context.Metadata.FrontCreateRouteValues = new RouteValueDictionary {
                    {"Area", "Contents"},
                    {"Controller", "FrontAdmin"},
                    {"Action", "Create"},
                    {"Id", context.ContentItem.ContentType}
                };
            }

            if (context.Metadata.FrontEditorRouteValues == null) {
                context.Metadata.FrontEditorRouteValues = new RouteValueDictionary {
                    {"Area", "Contents"},
                    {"Controller", "FrontAdmin"},
                    {"Action", "Edit"},
                    {"Id", context.ContentItem.Id}
                };
            }*/
            if (context.Metadata.DisplayRouteValues == null) {
                context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                    {"Area", "Contents"},
                    {"Controller", "Item"},
                    {"Action", "Display"},
                    {"Id", context.ContentItem.Id}
                };
            }
            if (context.Metadata.RemoveRouteValues == null) {
                context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                    {"Area", "Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Remove"},
                    {"Id", context.ContentItem.Id}
                };
            }
        }
    }
}