using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleNET;
using System.Data.Odbc;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;

namespace DayzWhitelisterPro
{
    class Program
    {
        // init our settings
        public static DZWLSettings dzwlSettings = new DZWLSettings("config.xml");

        static BattlEyeLoginCredentials logcred = new BattlEyeLoginCredentials { Host = dzwlSettings.beHost, Password = dzwlSettings.bePass, Port = Convert.ToInt32(dzwlSettings.bePort) };
        static IBattleNET b = new BattlEyeClient(logcred);

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing DayZ Whitelister Pro");

            // init BattlEye
            b.MessageReceivedEvent += DumpMessage;
            b.DisconnectEvent += Disconnected;
            b.ReconnectOnPacketLoss(true);
            b.Connect();

            try
            {
                if (b.IsConnected() == false)
                {
                    Console.WriteLine("No connection To server");
                    Console.WriteLine("Exiting");
                    b = null;
                    return;
                }

                Console.WriteLine("Connected...");
                Console.WriteLine("Waiting for clients");

                // main body of work
                do
                {
                    // capture logs
                    // Wait 1 second to conserve CPU cycles.  Thanks ryan!
                    Thread.Sleep(1000);
                    

                } while (b.IsConnected() == true);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                b = null;
            }
        }

        private static void DumpMessage(BattlEyeMessageEventArgs args)
        {
            // echo message to console
            //Console.WriteLine(args.Message);

            try
            {
                Match matchString;

                // Grab the user data if it matches our regular expresion - Thanks to mmmmk for this Regex!
                matchString = Regex.Match(args.Message, @"Player #(?<player_id>[0-9]{1,3})\s(?<user>.+) - GUID: (?<guid>.+)\W\D\S", RegexOptions.IgnoreCase);
                if (matchString.Success)
                {
                    // new client obj
                    DayzClient client = new DayzClient();

                    client.GUID = matchString.Groups["guid"].Value.Trim(); // thanks DeanHyde
                    client.playerNo = Convert.ToInt32(matchString.Groups["player_id"].Value);
                    client.UserName = matchString.Groups["user"].Value;

                    // did we get a valid result? verify
                    if (client.GUID != null && client.UserName != null)
                    {
                        if (VerifyWhiteList(client) == false)
                        {
                            // user is not white listed kick and send message
                            KickPlayer(client);

                            // log event
                            client.logType = DayzClient.LogTypes.Kick;
                            LogPlayer(client);
                        }
                        else
                        {
                            // display welcome message
                            WelcomeMessage(client);

                            // log event
                            client.logType = DayzClient.LogTypes.Success;
                            LogPlayer(client);
                        }
                    }
                    
                    // destroy client obj
                    client = null;
                }
            }
            catch (Exception ex)
            {
                // do nothing
            }
        }

        private static void Disconnected(BattlEyeDisconnectEventArgs args)
        {
            Console.WriteLine(args.Message);
        }

        private static bool VerifyWhiteList(DayzClient client)
        {
            bool returnVal = false;

            string connStr = string.Format("server={0};user={1};database={2};port={3};password={4};", dzwlSettings.dbHost, dzwlSettings.dbUser, dzwlSettings.dbDatabase, dzwlSettings.dbPort, dzwlSettings.dbPass);

            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand cmd = new MySqlCommand();
            MySqlDataReader rdr = null;

            try
            {
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "proc_CheckWhiteList";

                cmd.Parameters.Add(new MySqlParameter("p_guid", client.GUID));

                rdr = cmd.ExecuteReader();

                if (rdr.HasRows == true)
                {
                    returnVal = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                rdr.Close();
                conn.Close();
                rdr = null;
                conn = null;
                cmd = null;
                
            }
            return returnVal;
        }
        
        private static void WelcomeMessage(DayzClient client)
        {
            if (dzwlSettings.WelcomeMessageEnabled == true)
            {
                b.SendCommandPacket(EBattlEyeCommand.Say, string.Format("-1 Welcome: {0}", client.UserName));
            }

                Console.WriteLine(string.Format("Verified Player {0}: {1} - {2}", client.playerNo.ToString(), client.GUID.ToString(), client.UserName.ToString()));
        }
        private static void KickPlayer(DayzClient client)
        {
            if (dzwlSettings.beWhiteListEnabled == true)
            {
                b.SendCommandPacket(EBattlEyeCommand.Kick, string.Format(@"{0} Client not whitelisted! Visit {1} for whitelisting", client.playerNo.ToString(), dzwlSettings.URL));
                
            }
            Console.WriteLine(string.Format("Kicked Player {0} : {1} - {2}", client.playerNo.ToString(), client.GUID.ToString(), client.UserName.ToString()));
        }

        private static void LogPlayer(DayzClient client)
        {
            // call insert to log function
            string connStr = string.Format("server={0};user={1};database={2};port={3};password={4};", dzwlSettings.dbHost, dzwlSettings.dbUser, dzwlSettings.dbDatabase, dzwlSettings.dbPort, dzwlSettings.dbPass);

            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand cmd = new MySqlCommand();

            string queryString = string.Format("call proc_CheckWhiteList('{0}')", client.GUID);

            try
            {
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "proc_LogWhiteList";

                cmd.Parameters.Add(new MySqlParameter("p_name", client.UserName));
                cmd.Parameters.Add(new MySqlParameter("p_GUID", client.GUID));
                cmd.Parameters.Add(new MySqlParameter("p_logtype", Convert.ToInt32(client.logType)));

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                conn.Close();
                conn = null;
                cmd = null;
            }
        }
    }

    public class DayzClient
    {
        public string GUID { get; set; }
        public string IP { get; set; }
        public string UserName { get; set; }
        public string message { get; set; }
        public int playerNo { get; set; }

        public LogTypes logType { get; set; }

        public enum LogTypes
        {
            Success = 1,
            Kick = 2
        }
    }

    public class DZWLSettings
    {
        public string dbHost { get; set; }
        public string dbDatabase { get; set; }
        public string dbUser { get; set; }
        public string dbPass { get; set; }
        public string dbPort { get; set; }

        public string beHost { get; set; }
        public string bePort { get; set; }
        public string bePass { get; set; }
        public bool beWhiteListEnabled { get; set; }
        public string URL { get; set; }
        public bool WelcomeMessageEnabled { get; set; }

        public string configFileName { get; set; }

        // constructor -- Load our XML settings when we init our object
        public DZWLSettings(string cfgFile)
        {
            configFileName = cfgFile;
            this.LoadXMLSettings();
        }

        public void LoadXMLSettings()
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(this.configFileName);
                foreach (XmlNode node in xmlDoc)
                {
                    if (node.Name == "config")
                    {
                        foreach (XmlNode configNode in node)
                        {
                            if (configNode.Name == "section")
                            {
                                if (configNode.Attributes.Count == 1)
                                {
                                    switch (configNode.Attributes[0].Value)
                                    {
                                        case "DB":
                                            foreach (XmlNode dbNode in configNode)
                                            {
                                                if (dbNode.Attributes.Count == 1)
                                                {
                                                    switch (dbNode.Attributes[0].Value)
                                                    {
                                                        case "Host":
                                                            this.dbHost = dbNode.InnerText;
                                                            break;
                                                        case "Port":
                                                            this.dbPort = dbNode.InnerText;
                                                            break;
                                                        case "User":
                                                            this.dbUser = dbNode.InnerText;
                                                            break;
                                                        case "Pass":
                                                            this.dbPass = dbNode.InnerText;
                                                            break;
                                                        case "DB":
                                                            this.dbDatabase = dbNode.InnerText;
                                                            break;
                                                    }
                                                }
                                            }
                                            break;

                                        case "RCON":
                                            foreach (XmlNode rconNode in configNode)
                                            {
                                                if (rconNode.Attributes.Count == 1)
                                                {
                                                    switch (rconNode.Attributes[0].Value)
                                                    {
                                                        case "Host":
                                                            this.beHost = rconNode.InnerText;
                                                            break;
                                                        case "Port":
                                                            this.bePort = rconNode.InnerText;
                                                            break;
                                                        case "Pass":
                                                            this.bePass = rconNode.InnerText;
                                                            break;
                                                    }

                                                }
                                            }
                                            break;

                                        case "General":
                                            foreach (XmlNode rconNode in configNode)
                                            {
                                                if (rconNode.Attributes.Count == 1)
                                                {
                                                    switch (rconNode.Attributes[0].Value)
                                                    {
                                                        case "Enabled":
                                                            this.beWhiteListEnabled = Convert.ToBoolean(rconNode.InnerText);
                                                            break;
                                                        case "URL":
                                                            this.URL = rconNode.InnerText;
                                                            break;
                                                        case "WelcomeMessage":
                                                            this.WelcomeMessageEnabled = Convert.ToBoolean(rconNode.InnerText);
                                                            break;
                                                    }

                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // output error
                Console.WriteLine(ex);
                Console.WriteLine("Error in Config File!");
            }
            finally
            {
                xmlDoc = null;
            }
        }
    }
}
