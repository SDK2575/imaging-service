using System;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using System.Text;
using System.Data.SqlClient;

namespace EnterpriseImaging.ImagingServices.ServiceInterface
{
    /// <summary>
    /// Summary description for Logger.
    /// </summary>
    public class Logger
    {
        static EventLog MobjLog = new EventLog();

        static string MstrSource = "Imaging Web Service";

        private Logger()
        { }

        /// <summary>
        /// WriteToLog writes to the event log and the OnbaseDataFeeds tblServicesLog table.
        /// </summary>
        /// <param name="PstrErrorDescription">Brief description.</param>
        /// <param name="PstrErrorDetails">More details about the error</param>
        /// <param name="PstrErrorName">Identifies this web service as the source of the error</param>
        /// <param name="PstrImageID">The document id if it is known</param>
        public static void WriteToLog(string PstrErrorDescription, string PstrErrorDetails, string PstrErrorName, string PstrImageID)
        {
            string LstrErrorDetails = LimitSize(PstrErrorDetails.Replace("'", "''"), 1000);
            string LstrErrorName = LimitSize(PstrErrorName.Replace("'", "''"), 250);
            string LstrErrorDescription = LimitSize(PstrErrorDescription.Replace("'", "''"), 250);

            //Write to log table
            string LstrLogConnString = ConfigurationManager.AppSettings["LogConnectionString"];
            using (SqlConnection LconLogDB = new SqlConnection(LstrLogConnString))
            {
                LconLogDB.Open();
                StringBuilder LobjSQL = new StringBuilder();
                LobjSQL.Append("Insert into dbo.tblServicesLog ");
                LobjSQL.Append("(DateCreated, ErrorName,Description,ErrorDetails,ImageID)");
                LobjSQL.Append("Values(@DateCreated , ");
                LobjSQL.Append("@ErrorName, @Description, @ErrorDetails, ");
                LobjSQL.Append("@ImageID)");
                using (SqlCommand LcmdInsert = new SqlCommand(LobjSQL.ToString(), LconLogDB))
                {
                    LcmdInsert.Parameters.AddWithValue("@DateCreated", System.DateTime.Now);
                    LcmdInsert.Parameters.AddWithValue("@ErrorName", LstrErrorName);
                    LcmdInsert.Parameters.AddWithValue("@Description", LstrErrorDescription);
                    LcmdInsert.Parameters.AddWithValue("@ErrorDetails", LstrErrorDetails);
                    LcmdInsert.Parameters.AddWithValue("@ImageID", PstrImageID);
                    LcmdInsert.CommandTimeout = 0;
                    LcmdInsert.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Make sure that the data field lengths are not longer than the database columns.
        /// </summary>
        /// <param name="PstrStringIn">String to check</param>
        /// <param name="PiMaxLength">Max length of database table column</param>
        /// <returns></returns>
        public static string LimitSize(string PstrMsgIn, int PiMaxLength)
        {
            if (PstrMsgIn.Length > PiMaxLength)
            {
                return PstrMsgIn.Substring(0, PiMaxLength - 1);
            }
            else
            {
                return PstrMsgIn;
            }
        }


        private static void OpenLog()
        {
            if (!EventLog.SourceExists(MstrSource))
            {
                EventLog.CreateEventSource(MstrSource, "Imaging Web Service Error Log");
            }

            MobjLog.Source = MstrSource;

        }

        public static void LogMessage(string message)
        {
            OpenLog();
            MobjLog.WriteEntry(message, EventLogEntryType.Information);
        }

        public static void LogError(string message)
        {
            OpenLog();
            MobjLog.WriteEntry(message, EventLogEntryType.Error);
        }
    }
}
