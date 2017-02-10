using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace EventHubDemo.Common
{
    public class MetricEvent
    {
        [DataMember]
        public int DeviceId { get; set; }
        [DataMember]
        public int Temperature { get; set; }
    }

}
