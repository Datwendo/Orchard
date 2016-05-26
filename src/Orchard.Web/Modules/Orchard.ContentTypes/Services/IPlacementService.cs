using System.Collections.Generic;
using Orchard.ContentTypes.Settings;

namespace Orchard.ContentTypes.Services {
    public interface IPlacementService : IDependency {
        IEnumerable<DriverResultPlacement> GetDisplayPlacement(string contentType);
        IEnumerable<DriverResultPlacement> GetEditorPlacement(string contentType);
        // CS 25/5
        IEnumerable<DriverResultPlacement> GetFrontEditorPlacement(string contentType);
        IEnumerable<string> GetZones();
        IEnumerable<string> GetZones(string layer);
    }
}