using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.IO;

namespace DHPO_Auditor
{
    class Program
    {

        static void Main(string[] args)
        {
            Control();

        }

        public static string baseDir = ConfigurationManager.AppSettings.Get("baseDir");
        public static string[] global_start;
        public static string[] global_end;
        private static string dateformat = "yyyy-MM-dd HH:mm";

        //TransactionType 2 CS,4 PersonRegister,8 RA,16 PR,32 PA,-1 All
        //Direction 1 sent 2 received
        //TransactionStatus 1 Non-Downloaded 2 Downloaded

        public static void Control()
        {
            try
            {
                Logger.Info("Program Started");
                Logger.Info("Picking up values from configuration");

                bool provider = bool.Parse(ConfigurationManager.AppSettings.Get("Provider"));
                bool payer = bool.Parse(ConfigurationManager.AppSettings.Get("Payer"));

                bool Archived = bool.Parse(ConfigurationManager.AppSettings.Get("Archived"));
                bool Production = bool.Parse(ConfigurationManager.AppSettings.Get("Production"));

                string StartDate = ConfigurationManager.AppSettings.Get("StartDate");
                string EndDate = ConfigurationManager.AppSettings.Get("EndDate");

                Logger.Info("Reading Input file");
                string[] lines = File.ReadAllLines(baseDir + "Input.csv");


                if (provider)
                {
                    Logger.Info("Operation started for Provider");
                    List<DHPO_Facility> yo = Get_Creds(lines, provider);
                    for (int i = 0; i < yo.Count; i++)
                    {
                        Provider_Control(yo[i].username, yo[i].password, yo[i].license, StartDate, EndDate, Archived, Production);
                    }
                }

                if (payer)
                {
                    Logger.Info("Operation started for Payer");
                    List<DHPO_Facility> yo = Get_Creds(lines, payer);
                    for (int i = 0; i < yo.Count; i++)
                    {
                        Payer_Control(yo[i].username, yo[i].password, yo[i].license, StartDate, EndDate, Archived, Production);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
        }

        public static void Provider_Control(string username, string password, string license, string start_date, string end_date, bool isArchive, bool isProduction)
        {
            Logger.Info("Provider having license " + license + " is being executed");

            try
            {

                int direction = 0;
                int downloadstatus = 0;
                int transactionType = 0;
                string Date = start_date;
                Logger.Info("Getting the date range");
                get_DateRanges("60", Date, end_date);

                Logger.Info("Picking up more values from configuration");

                string PR = ConfigurationManager.AppSettings.Get("PR");
                string PA = ConfigurationManager.AppSettings.Get("PA");
                string CS = ConfigurationManager.AppSettings.Get("CS");
                string RA = ConfigurationManager.AppSettings.Get("RA");

                for (int i = 0; i < global_start.Length; i++) 
                {
                    Logger.Info("Finding records for date : " + global_start[i]);
                    if (isProduction)
                    {
                        Logger.Info("In Production");
                        if (bool.Parse(PR))//PR
                        {
                            direction = 1; //Sent Transaction
                            transactionType = 16; //Prior Request

                            Logger.Info("Searching Non-Downloaded PR");
                            downloadstatus = 1; //Non-Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);

                            Logger.Info("Searching Downloaded PR");
                            downloadstatus = 2; //Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(PA))//PA
                        {
                            direction = 2; //Received Transaction
                            transactionType = 32; //Prior Authorization

                            Logger.Info("Searching Non-Downloaded PA");
                            downloadstatus = 1; //Non-Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);

                            Logger.Info("Searching Downloaded PA");
                            downloadstatus = 2; //Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(CS))//CS
                        {
                            direction = 1; //Sent Transaction
                            transactionType = 2; //Claim Submission

                            Logger.Info("Searching Non-Downloaded CS");
                            downloadstatus = 1; //Non-Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);

                            Logger.Info("Searching Downloaded CS");
                            downloadstatus = 2; //Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(RA))//RA
                        {
                            direction = 2; //Received Transaction
                            transactionType = 8; //Remittance Authorization

                            Logger.Info("Searching Non-Downloaded RA");
                            downloadstatus = 1; //Non-Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);

                            Logger.Info("Searching Downloaded PR");
                            downloadstatus = 2; //Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }
                    }

                    if (isArchive)
                    {
                        Logger.Info("In Archive");
                        if (bool.Parse(PR))//PR
                        {
                            direction = 1; //Sent Transaction
                            transactionType = 16; //Prior Request

                            Logger.Info("Searching Non-Downloaded PR");
                            downloadstatus = 1; //Non-Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);

                            Logger.Info("Searching Downloaded PR");
                            downloadstatus = 2; //Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(PA))//PA
                        {
                            direction = 2; //Received Transaction
                            transactionType = 32; //Prior Authorization

                            Logger.Info("Searching Non-Downloaded PA");
                            downloadstatus = 1; //Non-Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);

                            Logger.Info("Searching Downloaded PA");
                            downloadstatus = 2; //Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(CS))//CS
                        {
                            direction = 1; //Sent Transaction
                            transactionType = 2; //Claim Submission

                            Logger.Info("Searching Non-Downloaded CS");

                            downloadstatus = 1; //Non-Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);

                            Logger.Info("Searching Downloaded CS");

                            downloadstatus = 2; //Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(RA))//RA
                        {
                            direction = 2; //Received Transaction
                            transactionType = 8; //Remittance Authorization

                            Logger.Info("Searching Non-Downloaded RA");

                            downloadstatus = 1; //Non-Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);

                            Logger.Info("Searching Downloaded RA");

                            downloadstatus = 2; //Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }
                    }

                }
                

            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
        }
        public static void Payer_Control(string username, string password, string license, string start_date, string end_date, bool isArchive, bool isProduction)
        {
            Logger.Info("Payer having license " + license + " is being executed");
            try
            {

                int direction = 0;
                int downloadstatus = 0;
                int transactionType = 0;
                string Date = start_date;

                Logger.Info("Getting the date range");
                get_DateRanges("60", Date, end_date);

                Logger.Info("Picking up more values from configuration");

                string PR = ConfigurationManager.AppSettings.Get("PR");
                string PA = ConfigurationManager.AppSettings.Get("PA");
                string CS = ConfigurationManager.AppSettings.Get("CS");
                string RA = ConfigurationManager.AppSettings.Get("RA");

                for (int i = 0; i < global_start.Length; i++)
                {
                    Logger.Info("Finding records for date : " + global_start[i]);
                    if (isProduction)
                    {
                        Logger.Info("In Production");
                        if (bool.Parse(PR))//PR
                        {
                            direction = 2; //Received Transaction
                            transactionType = 16; //Prior Request
                            Logger.Info("Searching Non-Downloaded PR");

                            downloadstatus = 1; //Non-Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);

                            Logger.Info("Searching Downloaded PR");
                      
                            downloadstatus = 2; //Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(PA))//PA
                        {
                            direction = 1; //Sent Transaction
                            transactionType = 32; //Prior Authorization
                            Logger.Info("Searching Non-Downloaded PA");

                            downloadstatus = 1; //Non-Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                            Logger.Info("Searching Downloaded PA");

                            downloadstatus = 2; //Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(CS))//CS
                        {
                            direction = 2; //Received Transaction
                            transactionType = 2; //Claim Submission
                            Logger.Info("Searching Non-Downloaded CS");
                            downloadstatus = 1; //Non-Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                            Logger.Info("Searching Downloaded CS");
                            downloadstatus = 2; //Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(RA))//RA
                        {
                            direction = 1; //Sent Transaction
                            transactionType = 8; //Remittance Authorization
                            Logger.Info("Searching Non-Downloaded RA");
                            downloadstatus = 1; //Non-Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                            Logger.Info("Searching Downloaded RA");
                            downloadstatus = 2; //Downloaded
                            DHPO_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }
                    }

                    if (isArchive)
                    {
                        Logger.Info("In Archive");
                        if (bool.Parse(PR))//PR
                        {
                            direction = 2; //Received Transaction
                            transactionType = 16; //Prior Request
                            Logger.Info("Searching Non-Downloaded PR");
                            downloadstatus = 1; //Non-Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                            Logger.Info("Searching Downloaded PR");
                            downloadstatus = 2; //Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(PA))//PA
                        {
                            direction = 1; //Sent Transaction
                            transactionType = 32; //Prior Authorization
                            Logger.Info("Searching Non-Downloaded PA");
                            downloadstatus = 1; //Non-Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                            Logger.Info("Searching Downloaded PA");
                            downloadstatus = 2; //Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(CS))//CS
                        {
                            direction = 2; //Received Transaction
                            transactionType = 2; //Claim Submission
                            Logger.Info("Searching Non-Downloaded CS");
                            downloadstatus = 1; //Non-Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                            Logger.Info("Searching Downloaded CS");
                            downloadstatus = 2; //Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }

                        if (bool.Parse(RA))//RA
                        {
                            direction = 1; //Sent Transaction
                            transactionType = 8; //Remittance Authorization
                            Logger.Info("Searching Non-Downloaded RA");
                            downloadstatus = 1; //Non-Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                            Logger.Info("Searching Downloaded RA");
                            downloadstatus = 2; //Downloaded
                            DHPOArchive_Search(username, password, license, direction, downloadstatus, transactionType, global_start[i], global_end[i]);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
        }

        private static int DHPO_Search(string username, string password, string license, int direction, int downloadstatus, int TransactionType, string SearchDateFrom, string SearchDateTo)
        {
            int i = 0;
            try
            {
                Logger.Info("DHPO Search method has been called");
                string foundTransactions = string.Empty;
                string errorMessage = string.Empty;
                string ePartner = "";

                DHPO.ValidateTransactionsSoapClient WS = new DHPO.ValidateTransactionsSoapClient();
                int result = WS.SearchTransactions(username, password, direction, license, ePartner, TransactionType, downloadstatus, string.Empty, SearchDateFrom, SearchDateTo, -1, -1, out foundTransactions, out errorMessage);

                if (foundTransactions != "<Files></Files>" && foundTransactions != "")
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(foundTransactions);
                    XmlNodeList FileIDs = xdoc.SelectNodes("//Files/File[@FileID]");

                    Logger.Info("Found " + FileIDs.Count + " transactions");

                    foreach (XmlNode node in FileIDs)
                    {
                        string FileID = node.Attributes["FileID"].InnerText;
                        DHPO_DownloadFile(username, password, FileID,GetNewDirName(license,TransactionType.ToString()));
                    }
                }
                Logger.Info("Transactions not found");

            }
            catch (Exception ex)
            {
                Logger.Info(ex);
                i = -1;
            }

            return i;
        }
        private static string DHPO_DownloadFile(string username, string password, string FileID,string Dir)
        {
            string result = string.Empty;
            byte[] file = null;
            string errorMessage = string.Empty;
            string filename = string.Empty;
            Logger.Info("Downloading FileID : " + FileID);
            try
            {
                DHPO.ValidateTransactionsSoapClient obj = new DHPO.ValidateTransactionsSoapClient();
                int WS_result = obj.DownloadTransactionFile(username, password, FileID, out filename, out file, out errorMessage);

                if (file != null)
                {
                    if (file.Length > 1)
                    {
                        ConvertFile(filename, file, Dir);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }

            return result;
        }

        private static int DHPOArchive_Search(string username, string password, string license, int direction, int downloadstatus, int TransactionType, string SearchDateFrom, string SearchDateTo)
        {
            int i = 0;
            try
            {
                Logger.Info("DHPO Archive Search method has been called");
                string foundTransactions = string.Empty;
                string errorMessage = string.Empty;
                string ePartner = "";

                DHPOArchive.ClaimsAndAuthorizationsArchiveSoapClient WS = new DHPOArchive.ClaimsAndAuthorizationsArchiveSoapClient();
                int result = WS.SearchTransactions(username, password, direction, license, ePartner, TransactionType, downloadstatus, string.Empty, SearchDateFrom, SearchDateTo, -1, -1, out foundTransactions, out errorMessage);

                if (foundTransactions != "<Files></Files>" && foundTransactions != "")
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(foundTransactions);

                    XmlNodeList FileIDs = xdoc.SelectNodes("//FileID");
                    Logger.Info("Found " + FileIDs.Count + " transactions");

                    foreach (string FileID in FileIDs)
                    {
                        DHPOArchive_DownloadFile(username, password, FileID, GetNewDirName(license, TransactionType.ToString()));
                    }
                }
                Logger.Info("Transactions not found");

            }
            catch (Exception ex)
            {
                Logger.Info(ex);
                i = -1;
            }

            return i;
        }
        private static string DHPOArchive_DownloadFile(string username, string password, string FileID,string Dir)
        {
            string result = string.Empty;
            byte[] file = null;
            string errorMessage = string.Empty;
            string filename = string.Empty;
            Logger.Info("Downloading FileID : " + FileID);
            try
            {
                DHPOArchive.ClaimsAndAuthorizationsArchiveSoapClient WS = new DHPOArchive.ClaimsAndAuthorizationsArchiveSoapClient();
                int WS_result = WS.DownloadTransactionFile(username, password, FileID, out filename, out file, out errorMessage);

                if (file.Length > 1)
                {
                    ConvertFile(filename, file, Dir);
                }

            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }

            return result;
        }

        private static void get_DateRanges(string MinutesToAdd, string Date, string LimitDate)
        {
            try
            {
                List<string> start_dates = new List<string>();
                List<string> end_dates = new List<string>();

                double minutes = Convert.ToDouble(MinutesToAdd);

                DateTime start = DateTime.ParseExact(Date, dateformat, null);
                DateTime end = start.AddMinutes(minutes);


                while (start <= Convert.ToDateTime(LimitDate))
                {
                    start_dates.Add(start.ToString(dateformat));
                    end_dates.Add(end.ToString(dateformat));

                    start = start.AddMinutes(minutes + 1);
                    end = start.AddMinutes(minutes);
                }
                global_start = start_dates.ToArray();
                global_end = end_dates.ToArray();

            }

            catch (Exception ex)
            {
                Logger.Info(ex);
            }

        }
        private static void ConvertFile(string filename, byte[] file,string dir)
        {
            try
            {
                Logger.Info("converting byte file FileName: " + filename);
                File.WriteAllBytes(dir + filename, file);
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
        }
        private static List<DHPO_Facility> Get_Creds(string[] lines, bool isprovider)
        {
            List<DHPO_Facility> po = new List<DHPO_Facility>();
            string query = string.Empty;

            Logger.Info("Taking credentials for the list of failities");

            try
            {

                string username = System.Configuration.ConfigurationManager.AppSettings.Get("username_DHPO");
                string password = System.Configuration.ConfigurationManager.AppSettings.Get("password_DHPO");
                string database = System.Configuration.ConfigurationManager.AppSettings.Get("database_DHPO");
                string IP = System.Configuration.ConfigurationManager.AppSettings.Get("IP_DHPO");
                string Connection = "Data Source=" + IP + ";Initial Catalog=" + database + ";User ID=" + username + ";Password=" + password + ";Connection Timeout=30;";


                if (isprovider)
                {
                    query = "select username,Password,LicenseID from provider where LicenseID in (" + split_array(lines) + ")";

                }
                else if (!isprovider)
                {
                    query = "select UserName,Password,PayerCode from Payers where PayerCode in (" + split_array(lines) + ")";

                }
                Logger.Info("Executing query: " + query);

                DataTable dt = Execute_Query(Connection, query);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            po.Add(new DHPO_Facility { username = dt.Rows[i][0].ToString(), password = dt.Rows[i][1].ToString(), license = dt.Rows[i][2].ToString() });
                        }
                    }
                }

                Logger.Info("Execution Successful");

            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }

            return po;
        }
        static string split_array(string[] data)
        {
            string result = string.Empty;
            try
            {
                //string concat = null;
                StringBuilder cot = new StringBuilder();
                foreach (string s in data)
                {
                    cot.Append(string.Format("'" + s.Trim() + "',"));
                }
                //return concat.Remove(concat.Length - 1);
                result = Convert.ToString(cot).Remove(cot.Length - 1);
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
            return result;
        }
        private static DataTable Execute_Query(string connection, string query)
        {
            DataTable dt = new DataTable();
            try
            {
                Logger.Info("Running query : " + query);

                using (SqlConnection con = new SqlConnection(connection))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.SelectCommand.CommandTimeout = 1800;
                            da.Fill(dt);
                            Logger.Info("Query executed successfully");
                        }
                        con.Close();
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
                return null;
            }
        }
        private static string GetNewDirName(string license,string TransactionType)
        {
            string result = string.Empty;
            try
            {
                if(!Directory.Exists(baseDir+"Output\\"+license))
                {
                    Directory.CreateDirectory(baseDir + "Output\\" + license);
                    Directory.CreateDirectory(baseDir + "Output\\" + license + "\\PR");
                    Directory.CreateDirectory(baseDir + "Output\\" + license + "\\PA");
                    Directory.CreateDirectory(baseDir + "Output\\" + license + "\\CS");
                    Directory.CreateDirectory(baseDir + "Output\\" + license + "\\RA");
                }

                string str = string.Empty;

                switch (TransactionType)
                {
                    case "16":
                        str = "PR";
                        break;
                    case "32":
                        str = "PA";
                        break;
                    case "8":
                        str = "RA";
                        break;
                    case "2":
                        str = "CS";
                        break;
                }


                result = baseDir + "Output\\" + license + "\\" + str + "\\";
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
            return result;
        }
    }

    public class DHPO_Facility
    {
        public string username { get; set; }
        public string password { get; set; }
        public string license { get; set; }

    }

}
