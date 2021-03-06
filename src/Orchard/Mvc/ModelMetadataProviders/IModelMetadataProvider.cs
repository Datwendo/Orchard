﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;

// CS 26/6 Harmony

namespace Orchard.Mvc.ModelMetadataProviders {
    public interface IModelMetadataProvider : IDependency {
        IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType);
        ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName);
        ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType);
    }
}
