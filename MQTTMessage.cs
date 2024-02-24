using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreHWM_MQTT
{
    internal class MQTTMessage
    {
        public string Topic { get; set; }
        public string Payload { get; set; }
    }
}
