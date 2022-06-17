using System;
using System.IO.Ports;
using System.Configuration;
using Mahas.Helpers;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using serialport.Mahas.Helpers;

namespace serialport
{
    class Program
    {
        public static void Main()
        {
            while (true)
            {
                try
                {
                    // get serial config
                    var portName = ConfigurationManager.AppSettings["PortName"];
                    var baudRate = ConfigurationManager.AppSettings["BaudRate"];
                    var dataBits = ConfigurationManager.AppSettings["DataBits"];

                    var serialPort = new SerialPort(portName)
                    {
                        BaudRate = int.Parse(baudRate),
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        DataBits = int.Parse(dataBits),
                        Handshake = Handshake.None
                    };

                    // receive
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                    // open port
                    Console.WriteLine("Connecting...");
                    serialPort.Open();
                    Console.WriteLine("Connected");

                    // close app
                    while (true)
                    {
                        Console.Write("type exit to close the application : ");
                        if (Console.ReadLine().ToUpper() == "EXIT")
                        {
                            break;
                        }
                    }

                    serialPort.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.Write("close application (y/n) : ");
                if (Console.ReadLine().ToUpper() == "Y")
                {
                    break;
                }
            }
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            ProcessString(sp.ReadExisting());
        }

        private static void ProcessString(string stringData)
        {
            try
            {
                var startChar = ConfigurationManager.AppSettings["StartChar"].ToLiteral();
                var endChar = ConfigurationManager.AppSettings["EndChar"].ToLiteral();

                foreach (var x in stringData.Split(endChar))
                {
                    var data = x.Trim();
                    if (!string.IsNullOrEmpty(startChar) && data.IndexOf(startChar) == 0)
                    {
                        SendToDb(data);
                    }
                }
            }
            catch (Exception ex)
            {
                MahasLog.Error(ex);
            }
        }

        private static async void SendToDb(string data)
        {
            try
            {
                var log = ConfigurationManager.AppSettings["Log"];
                if (log == "true") {
                    MahasLog.Log(data);
                }

                var server = ConfigurationManager.AppSettings["Server"];
                var database = ConfigurationManager.AppSettings["Database"];
                var user = ConfigurationManager.AppSettings["DbUser"];
                var pass = ConfigurationManager.AppSettings["DbPass"];
                var code = ConfigurationManager.AppSettings["Code"];

                var conString = $"Server={server};Database={database};User ID={user}; Password={pass};Integrated Security=False;Trusted_Connection=False;MultipleActiveResultSets=true;";
                using var s = new MahasConnection(conString);
                s.OpenTransaction();
                var query = @$"INSERT INTO trResults (
                    Code,
                    Message
                ) VALUES (
                    @Code,
                    @Message
                )";
                await s.ExecuteNonQuery(query, new List<SqlParameter>(){
                    new SqlParameter("@Message", data),
                    new SqlParameter("@Message", code),
                });
                s.CommitTransaction();
            }
            catch (Exception ex)
            {
                MahasLog.Error(ex);
            }
        }
    }
}

