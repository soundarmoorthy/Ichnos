using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace IEngineer.Studio.Extension.Ichnos
    {
    public class OutputWindowTraceListener : ITraceMessageListener
        {

        DTE2 dte;
        OutputWindowPane pane;
        TraceStreamOptions options;
        public OutputWindowTraceListener(TraceStreamOptions options)
            {
            this.options = options;
            dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
            pane = dte.ToolWindows.OutputWindow.ActivePane;

            }

        public OutputWindowTraceListener() :
            this(new TraceStreamOptions())
        {
        
        }

        public void WriteAsync(XElement element)
            {
            pane.OutputString(element.ToString());
            }

        public TraceStreamOptions GetTraceOptions()
            {
            return this.options;
            }
        }
    }
