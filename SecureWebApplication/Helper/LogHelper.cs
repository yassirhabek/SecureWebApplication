using System.Diagnostics;
using System.Net;

namespace SecureWebApplication.Helper
{
    public class LogHelper
    {
        
        EventLog eventLog;
        public LogHelper()
        {
            eventLog = new EventLog();
        }

        public void CreateLog(Exception exception, EventLogEntryType eventLogEntryType)
        {
            string message = "Source: " + exception.Source.ToString().Trim() + " \n" +
                "Method: " + exception.TargetSite.Name.ToString() + " \n" +
                "Date: " + DateTime.Now + "\n" +
                "Machine: " + Dns.GetHostName().ToString() + " \n" +
                "Error: " + exception.Message.ToString().Trim() + " \n" +
                "Stacktrace: " + exception.StackTrace.ToString().Trim() + " \n";

            eventLog.Source = "Application";
            eventLog.WriteEntry(message, eventLogEntryType);
        }
    }
}
