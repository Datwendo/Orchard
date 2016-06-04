using Orchard.ContentManagement;
using System.Web.Mvc;

namespace Orchard.Core.Common.Models {
    public class BodyPart : ContentPart<BodyPartRecord> {
        [AllowHtml] // CS 30/5
        public string Text {
            get { return Retrieve(x => x.Text); }
            set { Store(x => x.Text, value); }
        }

        public string Format {
            get { return Retrieve(x => x.Format); }
            set { Store(x => x.Format, value); }
        }
    }
}
