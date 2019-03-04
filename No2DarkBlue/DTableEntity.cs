using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace No2DarkBlue
{
    public class DTableEntity : TableEntity
    {
        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);

            foreach (var thisProperty in
                GetType().GetProperties().Where(thisProperty =>
                    thisProperty.GetType() != typeof(string) &&
                    properties.ContainsKey(thisProperty.Name) &&
                    (properties[thisProperty.Name].PropertyType == EdmType.String || properties[thisProperty.Name].PropertyType == EdmType.DateTime)))
            {

                var t = thisProperty.PropertyType;

                if (t.IsPrimitive || t == typeof(String))
                {
                    Convert.ChangeType(properties[thisProperty.Name].PropertyAsObject, thisProperty.PropertyType);
                }
                else if (t == typeof(DateTime?) || t == typeof(DateTime))
                {
                    if (properties[thisProperty.Name] != null)
                    {
                        thisProperty.SetValue(this, TimeZoneInfo.ConvertTimeFromUtc(properties[thisProperty.Name].DateTime.Value, TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id)));
                    }
                }
                else
                {
                    if (thisProperty.PropertyType.IsGenericType && (thisProperty.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                    {
                        var newStr = thisProperty.PropertyType.ToString().Replace("System.Collections.Generic.List`1[", "").Replace("]", "");
                        var type = Util.ObjectUtil.GetType(newStr);
                        Type listType = typeof(List<>).MakeGenericType(new Type[] { type });
                        thisProperty.SetValue(this, JsonConvert.DeserializeObject(properties[thisProperty.Name].StringValue, listType));
                    }
                    else
                    {
                        thisProperty.SetValue(this, JsonConvert.DeserializeObject(properties[thisProperty.Name].StringValue, Util.ObjectUtil.GetType(thisProperty.PropertyType.ToString())));
                    }

                }

            }


        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = base.WriteEntity(operationContext);

            foreach (var thisProperty in
                GetType().GetProperties().Where(thisProperty =>
                    !properties.ContainsKey(thisProperty.Name) &&
                    typeof(TableEntity).GetProperties().All(p => p.Name != thisProperty.Name)))
            {
                var value = thisProperty.GetValue(this);
                if (value != null)
                {
                    var t = thisProperty.GetType();
                    properties.Add(thisProperty.Name, new EntityProperty(JsonConvert.SerializeObject(value)));
                   
                }

            }

            return properties;
        }


    }
}
