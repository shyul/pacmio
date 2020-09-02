﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// Trade-Ideas API
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeIdeas.TIProData;
using TradeIdeas.TIProData.Configuration;
using TradeIdeas.ServerConnection;

namespace Pacmio.TIProData
{
    public static partial class Client
    {

        public static TimeSpan Ping { get; private set; }

        public static DateTime LastStatusCheckTime { get; private set; } = DateTime.MinValue;

        public static TimeSpan LastStatusCheck => (DateTime.Now - LastStatusCheckTime);

        public static AccountStatus AccountStatus { get; private set; }

        public static DateTime? NextPayment { get; private set; }

        public static int OddsmakerAvailable { get; private set; }

        public static bool Connected { get; private set; }

        public static void Connect()
        {
            Connected = false;
            Connection.LoginManager.Username = Root.Settings.TIUsername;
            Connection.LoginManager.Password = Root.Settings.TIPassword;
            Connection.ConnectionBase.ConnectionFactory = new TcpIpConnectionFactory(Root.Settings.TIServerAddress, Root.Settings.TIServerPort);
        }

        private static void PingUpdate(TimeSpan ping)
        {
            Ping = ping;

            Console.WriteLine("Ping: " + Ping.TotalMilliseconds.ToString() + "ms");
        }

        private static void AccountStatusUpdate(LoginManager source, AccountStatusArgs args)
        {
            AccountStatus = args.accountStatus;
            NextPayment = args.nextPayment;
            OddsmakerAvailable = args.oddsmakerAvailable;

            Console.WriteLine("Account Status:  " + AccountStatus.ToString() + " | Next Payment: " + ((NextPayment is DateTime pt) ? pt.ToString() : "Pay Now") + " | OddsMaker Remaining: " + OddsmakerAvailable.ToString());
        }

        private static void ConnectionBasePreview(ConnectionBase source, PreviewArgs args)
        {
            Console.WriteLine("GoodMessage: " + args.goodMessage.ToString());
            if (args.goodMessage)
            {
                Console.WriteLine(Encoding.ASCII.GetString(args.messageBody));
            }
        }

        private static void ConnectionBaseConnectionStatusUpdate(ConnectionBase source, ConnectionStatusCallbackArgs args)
        {
            Connected = !args.isServerDisconnect;
            string message = args.message;

            if(message.Contains("Working:  ")) 
            {
                message = message.Replace("Working:  ", "");
                LastStatusCheckTime = DateTime.Parse(message);
            }

            Console.WriteLine("Status: " + Connected + " | LastStatusCheck: " + LastStatusCheck.TotalSeconds.ToString() + " secs ago | Message: " + args.message);
        }

        public static void Initialize()
        {
            Connected = false;
            Connection.PingManager.PingUpdate += PingUpdate;
            Connection.LoginManager.AccountStatusUpdate += AccountStatusUpdate;
            Connection.ConnectionBase.Preview += ConnectionBasePreview;
            Connection.ConnectionBase.ConnectionStatusUpdate += ConnectionBaseConnectionStatusUpdate;
        }

        private static ConnectionMaster Connection { get; } = new ConnectionMaster();
    }
}
