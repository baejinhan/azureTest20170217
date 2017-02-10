using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeartbeatEHListener
{
    public class HeartbeatEntity : TableEntity
    {
        private string _deviceID;
        public string Device_ID
        {
            get
            {
                return _deviceID;
            }

            set
            {
                _deviceID = value;
                this.RowKey = _deviceID;
                //3 digit for example - CESCO001
                this.PartitionKey = _deviceID.Substring(5);
            }
        }
        public DateTime LASTDATARECIEVED { get; set; }
        private ControllerEntity _deviceData;
        public ControllerEntity DeviceData
        {
            get
            {
                return _deviceData;
            }
            set
            {
                _deviceData = value;
            }
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            //return base.WriteEntity(operationContext);
            var properties = base.WriteEntity(operationContext);

            // Iterating through the properties of the entity
            foreach (var property in GetType().GetProperties().Where(property =>
                    // Excluding already serialized props
                    !properties.ContainsKey(property.Name) &&
                    // Excluding internal TableEntity props
                    typeof(TableEntity).GetProperties().All(p => p.Name != property.Name)))
            {
                var value = property.GetValue(this);
                if (value != null)
                    // Serializing property to JSON
                    properties.Add(property.Name, new EntityProperty(JsonConvert.SerializeObject(value)));
            }

            return properties;

        }
        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);

            // Iterating through the properties of the entity
            foreach (var property in GetType().GetProperties().Where(property =>
                    // Excluding props which were not originally serialized
                    properties.ContainsKey(property.Name) &&
                    // Excluding props with target type of string (they are natively supported)
                    property.GetType() != typeof(string) &&
                    // Excluding non-string table fields (this will filter-out 
                    // all the remaining natively supported props like byte, DateTime, etc)
                    properties[property.Name].PropertyType == EdmType.String))
            {
                // Checking if property contains a valid JSON
                var jToken = TryParseJson(properties[property.Name].StringValue);
                if (jToken != null)
                {
                    // Constructing method for deserialization 
                    var toObjectMethod = jToken.GetType().GetMethod("ToObject", new[] { typeof(Type) });
                    // Invoking the method with the target property type; eg, jToken.ToObject(CustomType)
                    var value = toObjectMethod.Invoke(jToken, new object[] { property.PropertyType });

                    property.SetValue(this, value);
                }
            }
        }

        private static JToken TryParseJson(string s)
        {
            try { return JToken.Parse(s); }
            catch (JsonReaderException) { return null; }
        }



    }
}
