using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using StackFrame = EnvDTE.StackFrame;
using IEngineer.Studio.Extensions.Ichnos;
using System.Text;

namespace IEngineer.Studio.Extension.Ichnos
    {
    public sealed class TraceManager
        {

        static TraceManager manager;

        static TraceManager()
            {
            manager = new TraceManager();
            }

        public static TraceManager Instance
            {
                get
                {
                return manager;
                }
            }



        /// <summary>
        /// If the private member <paramref name="dte"/> is initialized set this to true
        /// </summary>
        private bool dteInitialized = false;

        /// <summary>
        /// The list of trace symbols that will be traced
        /// </summary>
        private List<string> symbols;
        private List<ITraceMessageListener> traceListeners;
        bool traceMessagePumpInitialized = false;
        Queue<TraceMessageSourceEntry> messages;

        private bool disableTraceOnDebugSessionEnd = true;
        private bool subscribedForDebuggerBreak = false;

        /// <summary>
        /// Initializes a new instance of Trace Manager
        /// </summary>
        /// <param name="settings">A list of symbol information to trace</param>
        /// <param name="outputStreamCollection">An enumerable collection of output stream to send the trace data</param>
        /// <param name="disableTraceOnDebugSessionEnd">If true, the tracing option will be disabled automatically, when the debug session has finished.</param>
        private TraceManager()
            {

            symbols = new List<string>();
            this.traceListeners = new List<ITraceMessageListener>();
            InitializeDTE();
           
            StatusReporter.Report(Resources.MsgTraceInitialized, Status.INFO);
            }
        

       public void Disable()
            {
                    debuggerEvents.OnEnterBreakMode -= new _dispDebuggerEvents_OnEnterBreakModeEventHandler(DebuggerEvents_OnEnterBreakMode);
                    StatusReporter.Report(Resources.MsgBreakEventHandlerUnsubscribed, Status.INFO);
            }

        public void Enable()
            {
            SubscribeForDebuggerEvents();
            }


        public void SetTraceContext(IEnumerable<string> symbols, IEnumerable<ITraceMessageListener> listeners)
            {
            if (traceSessionInProgress)
                throw new InvalidOperationException(Resources.ErrTraceSessionInProgress);
            this.Enable();

            InitializeTraceMessagePump();
            
            lock (this.symbols)
                {
                this.symbols = new List<String>(symbols);
                }
            lock (this.traceListeners)
                {
                this.traceListeners = new List<ITraceMessageListener>(listeners);
                }
            }

        System.Threading.Thread traceMessgePumpThread;
        private void InitializeTraceMessagePump()
            {
            if (traceMessagePumpInitialized)
                {
                StatusReporter.Report(Resources.MsgMessagePumpAlreadyInitialized, Status.INFO);
                return;
                }

            messages = new Queue<TraceMessageSourceEntry>();
            traceMessgePumpThread = new System.Threading.Thread(new ThreadStart(WriteToTraceListener));
            traceMessgePumpThread.Start();
            }


        void InitializeDTE()
            {
            dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            if (dte == null)
                {
                dteInitialized = false;
                throw new NullReferenceException(Resources.MsgDTEInitializedFailed);
                }
            
            dteInitialized = true;
            }

        private DTE dte;
        private DebuggerEvents debuggerEvents;
        private bool unsubscribeOnDebugSessionEnd;

        /// <summary>
        /// Enables the Trace manager to register for Debugger break events;
        /// </summary>
        /// <param name="unsubscribeOnDebugSessionEnd"></param>
        void SubscribeForDebuggerEvents()
            {
            if (subscribedForDebuggerBreak)
                {
                StatusReporter.Report(Resources.MsgAlreadySubscribedForDbgBreak, Status.INFO);
                return;
                }

            if (!dteInitialized)
                InitializeDTE();

            //Cache the debuggerEvents object so that it is not disposed by the shell.
            debuggerEvents = dte.Events.DebuggerEvents;
            if (debuggerEvents == null)
                throw new NullReferenceException(Resources.MsgDebuggerEventsObjNull);

            debuggerEvents.OnEnterBreakMode += new EnvDTE._dispDebuggerEvents_OnEnterBreakModeEventHandler(DebuggerEvents_OnEnterBreakMode);
            debuggerEvents.OnEnterDesignMode += new _dispDebuggerEvents_OnEnterDesignModeEventHandler(debuggerEvents_OnEnterDesignMode);
            StatusReporter.Report(Resources.MsgDbgBreakEventSubscribeSuccess, Status.INFO);
            subscribedForDebuggerBreak = true;
            }

        void debuggerEvents_OnEnterDesignMode(dbgEventReason Reason)
            {
                if (Reason == dbgEventReason.dbgEventReasonEndProgram || Reason == dbgEventReason.dbgEventReasonDetachProgram || Reason == dbgEventReason.dbgEventReasonStopDebugging)
                    {
                   }
            traceSessionInProgress = false;
            }

        bool traceSessionInProgress =true;

        void DebuggerEvents_OnEnterBreakMode(EnvDTE.dbgEventReason Reason, ref EnvDTE.dbgExecutionAction ExecutionAction)
            {
            try
                {
                traceSessionInProgress = true;
                Debugger debugger;
                lock (dte)
                    {
                    //Don't lock too much, just what is required.
                    debugger = dte.Debugger;
                    }

                if (Reason == dbgEventReason.dbgEventReasonBreakpoint)
                    {
                    string[] symbols;
                    lock (this.symbols)
                        {
                        //Clone the list to be used locally.
                        symbols = new string[this.symbols.Count];
                        this.symbols.CopyTo(symbols);
                        }
                    foreach (var symbol in symbols)
                        {
                        if (!string.IsNullOrEmpty(symbol))
                            {
                            try
                                {
                                var expression = debugger.GetExpression(symbol);
                                if (expression.IsValidValue)
                                    {
                                    var breakpoint = debugger.BreakpointLastHit;
                                    var stackFrame = debugger.CurrentStackFrame;
                                    var traceMessageSource = new TraceMessageSourceEntry() { Expression = expression, Breakpoint = breakpoint, StackFrame = stackFrame };
                                    lock (messages)
                                        {
                                        messages.Enqueue(traceMessageSource);
                                        }
                                    }
                                else
                                    {
                                    string breakpointContext;
                                    var bp = dte.Debugger.BreakpointLastHit;
                                    breakpointContext = string.Format(Resources.MsgBreakpointContextFormat, expression.Name, bp.FunctionName, bp.FileLine);
                                    StatusReporter.Report(string.Format(Resources.ErrTraceSymbolInvalid, breakpointContext), Status.WARNING);
                                    }
                                }
                            catch (Exception ex)
                                {
                                StatusReporter.Report(string.Format(Resources.ErrorEvaulatingSymbol, symbol + ex.ToString()), Status.WARNING);
                                }
                            }
                        }
                    }
                }
            finally
                {
                //This step makes sure that the execution continues after tracing this value;
                ExecutionAction = dbgExecutionAction.dbgExecutionActionGo;
                }
            }

        private class TraceMessageSourceEntry
            {
            public TraceMessageSourceEntry()
                {

                }
            internal Expression Expression;
            internal Breakpoint Breakpoint;
            internal StackFrame StackFrame;
            }

        private void WriteToTraceListener()
            {
            var messages = GetAllMessagesFromQueue();
            foreach (var message in messages)
                {
                foreach (var traceListener in traceListeners)
                    {
                    var messageAsXml = ComposeXmlMessage(message, traceListener.GetTraceOptions());
                    lock (traceListener)
                        {
                        try
                            {
                            traceListener.WriteAsync(messageAsXml);
                            }
                        catch (Exception ex)
                            {
                            StatusReporter.Report(string.Format(Resources.ErrTraceListenerThrowsExceptionOnWrite, ex.ToString()), Status.ERROR);
                            }
                        }
                    }
                }
            }

        private XElement ComposeXmlMessage(TraceMessageSourceEntry message, TraceStreamOptions options)
            {
            XElement element = new XElement(XName.Get(Names.TraceEntry));

            if (options.IncludeDateTime)
                {
                element.Add(new XElement(XName.Get(Names.TimeStamp)), DateTime.Now.ToString());
                }
            if (options.IncludeColumnNumber)
                {
                var colNumber = new XElement(XName.Get(Names.FileColumn), message.Breakpoint.FileColumn);
                element.Add(colNumber);
                }
            if (options.IncludeFileName)
                {
                var fileName = new XElement(XName.Get(Names.FileName), message.Breakpoint.File);
                element.Add(fileName);                
                }
            if (options.IncludeFunctionName)
                {
                var functionName = new XElement(XName.Get(Names.FunctionName), message.Breakpoint.FunctionName);
                element.Add(functionName);
                }
            if (options.IncludeLineNumber)
                {
                var lineNumber = new XElement(XName.Get(Names.FileLine), message.Breakpoint.FileLine);
                element.Add(lineNumber);
                }
            if (options.IncludeLocalVariables)
                {
                var localElement = new XElement(XName.Get(Names.Locals));
                element.Add(localElement);
                foreach (Expression variable in message.StackFrame.Locals)
                    {
                    var localVariable = new XElement(XName.Get(Names.LocalVariableEntry));
                    localVariable.SetAttributeValue(XName.Get(Names.LocalVariableEntryAttributeName), variable.Name);
                    localVariable.SetAttributeValue(XName.Get(Names.LocalVariableEntryAttributeValue), variable.Value);
                    }
                }
            return element;
            }

        private IEnumerable<TraceMessageSourceEntry> GetAllMessagesFromQueue()
            {
            List<TraceMessageSourceEntry> messageList = new List<TraceMessageSourceEntry> ();
            TraceMessageSourceEntry message = null;
            lock (messages)
                {
                while (messages.Any())
                    {
                        message  = messages.Dequeue();
                        messageList.Add(message);
                    }
                }
            return messageList.AsEnumerable();
            }
        }
    }