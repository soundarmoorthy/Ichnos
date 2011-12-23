using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace IEngineer.Studio.Extension.Ichnos
    {
    public interface ITraceMessageListener
        {
        /// <summary>
        /// This method will be fired asynchronously by the Trace manager. 
        /// </summary>
        /// <param name="xelement">The XElement format of the trace data</param>
        void WriteAsync(XElement element);
        
        /// <summary>
        /// Gets a list of options to take into consideration, while generating trace data.
        /// </summary>
        /// <returns></returns>
        TraceStreamOptions GetTraceOptions();
        }
    }
