﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;


namespace DSP_Server
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public TcpListener server;
        public bool listening;
        public int port;
        public Thread listenerThread;
        public static int clientNum;

        private void Form1_Load(object sender, EventArgs e)
        {
            listening = true;
            port = 1025;
        }

        private void startServer_Click(object sender, EventArgs e)
        {
            // Initialize Server
            server = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            server.Start();
            status.Text = status.Text + "Server Start !!" + Environment.NewLine;
            // Initialize Listener
            listenerThread = new Thread(new ThreadStart(Listening));
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        private void StopServer_Click(object sender, EventArgs e)
        {
            // Stop Server
            listening = false;
            server.Stop();
            server = null;
            status.Text = status.Text + "Server Stop !!" + Environment.NewLine;
            // Listener is OFF
            listenerThread.Abort();
            listenerThread = null;
        }

        private void Listening()
        {
            while (listening)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    new Thread(new ThreadStart(() => HandleClient(client))).Start();
                    clientNum++;
                }
                catch (Exception ex)
                {
                    Console.Write("No Client Connection !!");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void HandleClient(TcpClient client)
        {
            status.Invoke(new Action(() => status.Text = status.Text + "new Client" + Environment.NewLine));
            StreamReader sr = new StreamReader(client.GetStream());
            char[] data = new char[20];

            JObject message;
            string createText;

            while (true)
            {
                if ((sr.Read(data, 0, data.Length)) > 0)
                {
                    message = JObject.Parse(new string(data));
                    createText = message["lux"].ToString() + Environment.NewLine;
                    File.AppendAllText("client_" + clientNum + ".xls", createText);
                }
                else
                {
                    status.Invoke(new Action(() => status.Text = status.Text + "Client Disconnected !!" + Environment.NewLine));
                    break;
                }
                sr.DiscardBufferedData();
            }
        }
    }
}
