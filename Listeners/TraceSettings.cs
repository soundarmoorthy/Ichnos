using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IEngineer.Studio.Extensions.Ichnos;

namespace IEngineer.Studio.Extension.Ichnos
    {
    internal sealed class TraceSymbols
        {
        private IList<string> symbols;
        private bool canUpdateSymbolList = true;

        public TraceSymbols()
            {
            symbols = new List<string>();
            }

        public void AddSymbol(string symbolName)
            {
            if (!canUpdateSymbolList)
                {
                    StatusReporter.Report(Resources.ErrCannotUpdateSymbolList, Status.ERROR);
                throw new InvalidOperationException(Resources.ErrCannotUpdateSymbolList);
                }
            lock (symbols)
                {
                this.symbols.Add(symbolName);
                }
            }


        internal void RestictSymbolUpdate()
            {
            canUpdateSymbolList = false;
            }

        internal void AllowSymbolUpdate()
            {
            canUpdateSymbolList = true;
            }

        public void AddSymbols(IEnumerable<string> symbolNames)
            {
            try
                {
                foreach (var symbolName in symbolNames)
                    {
                    AddSymbol(symbolName);
                    }
                }
            catch (Exception ex)
                {
                throw new Exception(Resources.ErrorAddingSymbol, ex);
                }
            }


        internal IEnumerable<string> GetSymbols()
            {
            if (symbols != null && symbols.Any())
                {
                StatusReporter.Report(Resources.MsgSymbolsReturnedSuccesfully, Status.INFO);
                return symbols;
                }
            else
                {
                throw new InvalidOperationException(Resources.ErrorSymbolSourceIsNullOrEmpty);
                }
            }
        }
    }
