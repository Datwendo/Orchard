using Orchard.ContentManagement;

namespace Orchard.Services {
    public interface IHtmlFilter : IDependency {
        // CS 20/6
        string ProcessContent(string text, string flavor, ContentItem Item=null);

    }
}