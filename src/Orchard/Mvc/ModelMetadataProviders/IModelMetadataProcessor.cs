using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.Mvc.ModelMetadataProviders {
    // CS 30/6 Hamony7
    public class ModelMetadataContext {
        public ModelMetadata ModelMetadata { get; set; }

        public IList<Attribute> Attributes { get; set; }
        public Type ContainerType { get; set; }
        public Func<object> ModelAccessor { get; set; }
        public Type ModelType { get; set; }
        public string PropertyName { get; set; }
    }

    public interface IModelMetadataProcessor : ISingletonDependency {
        void ProcessMetadata(ModelMetadataContext context);
    }
}
