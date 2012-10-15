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

                // main body of work
                do
                {
                    // capture logs

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
            // new clien tobj
            DayzClient client = new DayzClient();

            // echo message to console
            Console.WriteLine(args.Message);

            try
            {
                Match matchString;

                // Grab the user data if it matches our regular expresion - Thanks to mmmmmkay for this Regex!
                matchString = Regex.Match(args.Message, @"Verified GUID\s\W(?<guid>.+)\W\sof player #(?<player_id>[0-9]{1,3})\s(?<user>.+)", RegexOptions.IgnoreCase);
                if (matchString.Success)
                {
                    client.GUID = matchString.Groups["guid"].Value;
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

                            // log event;
                            client.logType = DayzClient.LogTypes.Success;
                            LogPlayer(client);
                        }
                    }
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
                conn.Close();
            }
            return returnVal;
        }
        
        private static void WelcomeMessage(DayzClient client)
        {
            if (dzwlSettings.beWhiteListEnabled == true)
            {
                b.SendCommandPacket(EBattlEyeCommand.Say, string.Format(@"{0} Client Whitelist Verified", client.playerNo));
                b.SendCommandPacket(EBattlEyeCommand.Say, string.Format(@"{0} Welcome to our server!", client.playerNo));
            }
        }
        private static void KickPlayer(DayzClient client)
        {
            if (dzwlSettings.beWhiteListEnabled == true)
            {
                b.SendCommandPacket(EBattlEyeCommand.Say, string.Format(@"{0} Client not whitelisted! Visit http://big-t for whitelisting", client.playerNo));
                b.SendCommandPacket(EBattlEyeCommand.Kick, string.Format("{0} ", client.playerNo.ToString()));
            }
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

        public string configFileName { get; set; }

        // constructor -- Load our XML settings when we init our object
        public DZWLSettings(string cfgFile)
        {
            configFileName = cfgFile;
            this.LoadXMLSettings();
        }

        public void LoadXMLSettings()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
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
                                                        case "Enabled":
                                                            this.beWhiteListEnabled = Convert.ToBoolean(rconNode.InnerText);
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
                // do nothing
                Console.WriteLine("Error in Config File!");
            }
            finally
            {
            }
        }
    }
}
