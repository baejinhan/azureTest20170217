using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartbeatEHListener
{
    public interface IControllerData
    {
        int C002 { get; set; } //Power
        int C003 { get; set; } //FanSpeed
        int C004 { get; set; } //Timer
        int C005 { get; set; } //Ionizer
        int C006 { get; set; } //Scent
    }
}
