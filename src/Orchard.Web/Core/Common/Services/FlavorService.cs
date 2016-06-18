using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.Caching;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Common.Services {
    public class FlavorService : IFlavorService {
        private readonly Func<IShapeTableLocator> _shapeTableLocator;
        private readonly IWorkContextAccessor _wca;
        private readonly ICacheManager _cacheManager;

        public FlavorService(Func<IShapeTableLocator> shapeTableLocator, IWorkContextAccessor wca, ICacheManager cacheManager) {
            _shapeTableLocator = shapeTableLocator;
            _wca = wca;
            _cacheManager = cacheManager;
        }

        public IList<string> GetFlavors() {
            return _cacheManager.Get("Flavors", context => {
                var shapeTable = _shapeTableLocator().Lookup(_wca.GetContext().CurrentTheme.Id);
                var flavors = shapeTable.Bindings.Keys
                    .Where(x => x.StartsWith("Body_Editor__", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Substring("Body_Editor__".Length))
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .Select(x => x[0].ToString(CultureInfo.InvariantCulture).ToUpper() + x.Substring(1))
                    .Select(x => x.CamelFriendly());
                // CS 28/5 3/6
                //if (!flavors.Any()) {
                var flavors2 = shapeTable.Bindings.Keys
                        .Where(x => x.StartsWith("Body_FrontEditor__", StringComparison.OrdinalIgnoreCase))
                        .Select(x => x.Substring("Body_FrontEditor__".Length))
                        .Where(x => !String.IsNullOrWhiteSpace(x))
                        .Select(x => x[0].ToString(CultureInfo.InvariantCulture).ToUpper() + x.Substring(1))
                        .Select(x => x.CamelFriendly());
                //}

                var lst = flavors.ToList();
                foreach (var fl in flavors2) {
                    if (!lst.Contains(fl) )
                        lst.Add(fl);
                }
                return lst;
            });
        }
    }
}