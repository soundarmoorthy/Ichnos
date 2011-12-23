using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IEngineer.Studio.Extension.Ichnos
    {
    public class TraceStreamOptions
        {
        public TraceStreamOptions()
            {
            this.IncludeFunctionName = true;
            this.IncludeLineNumber = true;
            this.IncludeFileName = true;
            }

        public bool IncludeDateTime { get; set; }

        public bool IncludeFunctionName { get; set; }

        public bool IncludeLocalVariables { get; set; }

        public bool IncludeFileName { get; set; }

        public bool IncludeLineNumber { get; set; }

        public bool IncludeColumnNumber { get; set; }

        }
    }
