using System;
using System.Web.ApplicationServices;
using Orchard.Services;
using Orchard.ContentManagement;

namespace Markdown.Services {
    public class MarkdownFilter : IHtmlFilter {
        public string ProcessContent(string text, string flavor,ContentItem item) {
            return String.Equals(flavor, "markdown", StringComparison.OrdinalIgnoreCase) ? MarkdownReplace(text) : text;
        }

        private static string MarkdownReplace(string text) {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var markdown = new MarkdownSharp.Markdown();

            return markdown.Transform(text);
        }
    }
}