using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;
using IEngineer.Studio.Extension.Ichnos;
using IEngineer.Studio.Extensions.Ichnos;

namespace IEngineer.Studio.Extension.Ichnos
    {
    internal static class StatusReporter
        {
        static IVsUIShell uiShell;
        static StatusReporter()
            {
            uiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
            if (uiShell == null)
                {
                MessageBox.Show(Resources.ErrorUnableToGetShellInstance, Resources.ExtensionTitle, MessageBoxButtons.OK);
                }
            }

        internal static void ReportStatusInteractive(string message, OLEMSGICON msgIcon)
            {
            uint dwCompRole = 0; //Do not use, as suggested by msdn.
            var rclsidComp = Guid.Empty; //Do not use, as suggested by msdn.
            //F1 keyword that corresponds to a specific Help topic. For more information, see Unique F1 Keywords.
            var pszHelpFile = string.Empty;
            uint pszHelpContextId = 0; //Pass in zero.
            OLEMSGBUTTON msgbtn = OLEMSGBUTTON.OLEMSGBUTTON_OK;
            OLEMSGDEFBUTTON msgDefBtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
            //Sets the ALERT FLAG. State it as 0. Any error from this extension is not mission critical. So don't set the ALERT FLAG
            var fSysAlert = 0;
            //Return code.
            int pnResult  = 0;
            ActivityLog.LogError(Resources.ExtensionTitle, message);
            uiShell.ShowMessageBox(dwCompRole, ref rclsidComp, Resources.ExtensionTitle, message, pszHelpFile, pszHelpContextId, msgbtn, msgDefBtn, msgIcon, fSysAlert, out pnResult);
            }

        internal static void Report(string message, Status status)
            {
            try
                {
                switch (status)
                    {
                    case Status.INFO:
                    ActivityLog.LogInformation(Resources.ExtensionTitle, message);
                    break;

                    case Status.WARNING:
                    ActivityLog.LogWarning(Resources.ExtensionTitle, message);
                    break;

                    case Status.ERROR:
                    ActivityLog.LogError(Resources.ExtensionTitle, message);
                    break;

                    default:
                    ActivityLog.LogInformation(Resources.ExtensionTitle, message);
                    break;
                    }
                }
            catch (Exception)
                {
                //IF you are here then it's FUBAR. 
                }
            }
        }
    }
