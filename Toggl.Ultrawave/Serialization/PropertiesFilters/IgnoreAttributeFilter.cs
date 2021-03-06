﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Toggl.Ultrawave.Serialization
{
    internal sealed class IgnoreAttributeFilter<TIgnoredAttribute> : IPropertiesFilter
        where TIgnoredAttribute : IgnoreSerializationAttribute
{
        public IList<JsonProperty> Filter(IList<JsonProperty> properties)
        {
            foreach (JsonProperty property in properties)
            {
                var attributes = property.AttributeProvider.GetAttributes(typeof(TIgnoredAttribute), false);
                if (attributes.Any()) 
                    property.ShouldSerialize = _ => false;
            }

            return properties;
        }
    }
}
