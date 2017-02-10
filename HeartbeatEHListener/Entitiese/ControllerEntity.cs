using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartbeatEHListener
{
    public class ControllerEntity : IControllerData
    {
        public int C002 { get; set; } //Power
        public int C003 { get; set; } //FanSpeed
        public int C004 { get; set; } //Timer
        public int C005 { get; set; } //Ionizer
        public int C006 { get; set; } //Scent
    }
}
