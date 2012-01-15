using System;
using System.Linq;
using System.IO;


namespace IEngineer.Studio.Extension.Ichnos
{
    public class FileStreamTraceListener : ITraceMessageListener, IDisposable
    {

        StreamWriter writer;
        TraceStreamOptions options;
        /// <summary>
        /// Creates an instance of the FileStreamTraceListener with default TraceStreamOptions
        /// </summary>
        /// <param name="fileName">The file name to which the trace data will be written to. This is the full path to file.</param>
        public FileStreamTraceListener(string fileName)
            : this(fileName, new TraceStreamOptions())
        {

        }

        public FileStreamTraceListener(string fileName, TraceStreamOptions options)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Invalid file name", "fileName");
            if (Path.GetInvalidFileNameChars().Any() || Path.GetInvalidPathChars().Any())
                throw new ArgumentException("Invalid characters found in path", "fileName");
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                throw new DirectoryNotFoundException(string.Format("Unable to create a file stream for the file {0}", fileName));

            writer = new StreamWriter(fileName);
            this.options = options;
        }

        public void WriteAsync(System.Xml.Linq.XElement element)
        {
            writer.WriteLine(element);
        }

        public TraceStreamOptions GetTraceOptions()
        {
            return options;
        }


        public void Dispose()
        {
            if (writer != null)
                writer.Close();
        }

        ~FileStreamTraceListener()
        {
            if (writer != null)
                writer.Close();
        }
    }
}