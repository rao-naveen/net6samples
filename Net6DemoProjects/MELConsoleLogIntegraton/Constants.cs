using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MELConsoleLogIntegraton
{
    public static class Constants
    {
        //Trace,
        //Debug,
        //Information,
        //Warning,
        //Error,
        //Critical,
        //None
        // 0 - 2000 Information
        // 2001-3000 Service
        // 3001-4000 Usage
        // 6000-7000 Trace/Debug
        // 8000-9999 - Warning/Error/Critical/
        


        public static EventId ServiceEventId1 = new EventId(2001, "Service");
        public static EventId ServiceEventId2 = new EventId(2002, "Service");
        public static EventId UsageEventId1 = new EventId(3001, "Service");
        public static EventId UsageEventId2 = new EventId(3002, "Service");
    }
}

