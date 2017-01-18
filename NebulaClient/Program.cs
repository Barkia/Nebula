﻿using Nebula.Shared;
using System;
using System.ServiceModel;

namespace Nebula.Client
{
    class Program 
    {
        static void Main(string[] args)
        {
            NebulaMasterServiceCB hCb = new NebulaMasterServiceCB("localhost", 28000);
            hCb.Closed  += OnClosed;
            hCb.Faulted += OnFaulted;


            hCb.Service.Register(Environment.MachineName);
            
            System.Threading.Thread.CurrentThread.Join();
        }

        private static void OnClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Closed");
        }

        private static void OnFaulted(object sender, EventArgs e)
        {
            Console.WriteLine("Faulted");
        }
    }
}
