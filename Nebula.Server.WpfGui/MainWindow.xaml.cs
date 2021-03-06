﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nebula.Server;
using System.IO;
using Nebula.Shared;
using System.Net;

namespace Nebula.Server.WpfGui
{
    internal class NebulaClientWpf : NebulaClient
    {
    }

    public partial class MainWindow : Window
    {
        private NebulaMasterService<NebulaClientWpf> m_hService;
        private ReplModuleWindow m_hOutputWindow;

        private int m_iCurrentPort;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                m_iCurrentPort = int.Parse(Environment.GetCommandLineArgs()[0]);
            }
            catch (Exception)
            {
                m_iCurrentPort = 28000;
            }

            m_hTextPort.Text = m_iCurrentPort.ToString();

            m_hStatusLabel.Text = "Server Stopped";

            m_hClientList.ContextMenu = null;

            OnButtonStart(null, null);
        }

        private void OnClientDisconnected(NebulaClientWpf obj)
        {
            Dispatcher.BeginInvoke((Action)(() => 
            {
                m_hClientList.Items.Remove(obj);
                m_hStatusLabel.Text = $"{m_hClientList.Items.Count} Clients Connected";
            }));            
        }

        private void OnClientConnected(NebulaClientWpf obj)
        {
            Dispatcher.BeginInvoke((Action)(() => 
            {
                m_hClientList.Items.Add(obj);
                m_hStatusLabel.Text = $"{m_hClientList.Items.Count} Clients Connected";
            }));
        }

        private void OnContextMenuAddModule(object sender, RoutedEventArgs e)
        {
            NebulaClient hClient = m_hClientList.SelectedItem as NebulaClient;
            
            Microsoft.Win32.OpenFileDialog hDlg = new Microsoft.Win32.OpenFileDialog();
            hDlg.FileName = "Nebula Module";
            hDlg.DefaultExt = ".dll";
            hDlg.Filter = "Assembly Files (.dll)|*.dll";
   
            if (hDlg.ShowDialog() == true)
            {
                byte[] hData = File.ReadAllBytes(hDlg.FileName);

                NebulaModuleInfo[] hModules = hClient.Callback.AddModule(hData);                
            }
                        
        }

        private void OnContextMenuRemoveModule(object sender, RoutedEventArgs e)
        {
            NebulaClient hClient = m_hClientList.SelectedItem as NebulaClient;

            hClient.Callback.RemoveModule(Guid.NewGuid());
        }

        private void OnContextMenuListModules(object sender, RoutedEventArgs e)
        {
            NebulaClient hClient = m_hClientList.SelectedItem as NebulaClient;

            hClient.Callback.ListModules();
        }


        private void OnButtonStart(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_hButtonStart.Tag == null)
                {
                    m_iCurrentPort                   = int.Parse(m_hTextPort.Text);
                    m_hService                       = new NebulaMasterService<NebulaClientWpf>();
                    m_hService.ClientConnected      += OnClientConnected;
                    m_hService.ClientFaulted        += OnClientDisconnected;

                    m_hService.Start(m_iCurrentPort);
                    m_hButtonStart.Tag               = m_hService;
                    m_hButtonStartLabel.Content      = "Stop";
                    m_hTextPort.IsEnabled            = false;
                    m_hStatusLabel.Text              = "Server Started";
                    m_hService.ModuleDataReceived += OnModuleDataReceived;
                }
                else
                {
                    m_hService.ModuleDataReceived -= OnModuleDataReceived;
                    m_hService.Dispose();                    
                    m_hButtonStart.Tag               = null;
                    m_hService.ClientConnected      -= OnClientConnected;
                    m_hService.ClientFaulted        -= OnClientDisconnected;
                    m_hButtonStartLabel.Content      = "Start";
                    m_hTextPort.IsEnabled            = true;
                    m_hStatusLabel.Text              = "Server Stopped";
                    
                }
            }
            catch (Exception)
            {
            }            
        }

        private void OnModuleDataReceived(NebulaClientWpf hClient, Guid vModuleId, string hData)
        {
            m_hOutputWindow.Add(hData);
        }

        private void OnClientListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_hClientList.SelectedItem == null)
                m_hClientList.ContextMenu = null;
            else
                m_hClientList.ContextMenu = m_hClientContextMenu;
        }

        private void OnClientContextMenuOpen(object sender, ContextMenuEventArgs e)
        {
            NebulaClient hClient = m_hClientList.SelectedItem as NebulaClient;

            object first = m_hClientContextMenu.Items[0];
            object second = m_hClientContextMenu.Items[1];
            object third = m_hClientContextMenu.Items[2];
            object fourth = m_hClientContextMenu.Items[3];
            m_hClientContextMenu.Items.Clear();

            m_hClientContextMenu.Items.Add(first);
            m_hClientContextMenu.Items.Add(second);
            m_hClientContextMenu.Items.Add(third);
            m_hClientContextMenu.Items.Add(fourth);


            foreach (var item in hClient.Modules)
            {
                MenuItem hMenuItem = new MenuItem();                
                hMenuItem.Header = item.Name;

                m_hClientContextMenu.Items.Add(hMenuItem);

                foreach (var method in item.Methods)
                {
                    MenuItem hMethodItem = new MenuItem();
                    hMethodItem.Header = method.MethodName;
                    hMethodItem.Tag = item;
                    hMethodItem.Click += HMenuItem_Click1;

                    hMenuItem.Items.Add(hMethodItem);
                }                                
            }            
        }

        private void HMenuItem_Click1(object sender, RoutedEventArgs e)
        {
            NebulaClient hClient = m_hClientList.SelectedItem as NebulaClient;
            MenuItem item = sender as MenuItem;
            NebulaModuleInfo hModuleInfo = item.Tag as NebulaModuleInfo;

            m_hOutputWindow = new ReplModuleWindow(hClient, hModuleInfo.Guid);
            m_hOutputWindow.Show();
        }

    }
}
