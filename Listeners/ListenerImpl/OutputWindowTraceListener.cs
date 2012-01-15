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
using Microsoft.VisualStudio;

namespace IEngineer.Studio.Extension.Ichnos
{
    public class OutputWindowTraceListener : ITraceMessageListener
    {

        DTE2 dte;
        TraceStreamOptions options;
        IVsOutputWindowPane debugPane;
        public OutputWindowTraceListener(TraceStreamOptions options)
        {
            this.options = options;
            var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid debugPaneGuid = VSConstants.OutputWindowPaneGuid.DebugPane_guid;
            var result = outputWindow.GetPane(ref debugPaneGuid, out debugPane);
            if (result != VSConstants.S_OK)
            {
                throw new Exception("Unable to get an instance of the debug window pane");
            }
        }

        public OutputWindowTraceListener() :
            this(new TraceStreamOptions())
        {

        }

        public void WriteAsync(XElement element)
        {
            debugPane.OutputString(element.ToString());
        }

        public TraceStreamOptions GetTraceOptions()
        {
            return this.options;
        }
    }
}
