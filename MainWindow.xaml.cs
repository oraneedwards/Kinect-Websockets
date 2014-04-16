using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections;

using System.Security.Cryptography;
using System.Threading;

using System.Diagnostics;

using SocketIOClient;
using Newtonsoft.Json.Linq;

using System.Text.RegularExpressions;
using SocketIOClient.Messages;

namespace ShoushiPlatformApp 
{
        
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool attemptConnect = false;
        bool connected = false;
        bool errRec = false;
        bool messRec = false;

        String currWSMessage;         // the message that is currently being sent (to display on UI)
        String currWSStatus;         // the status of the WebSockets connection

        Client socket;

        public MainWindow()
        {            
            InitializeComponent();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            
            // ----------------------------------------------------------------------
            
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
          
        }
        
        //Websockets
        //https://github.com/sta/websocket-sharp

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            // After Kinect and Everything is loaded an initialized
            attemptConnect = true;
                        
            socket = new Client("http://soushi_command_server.nodejitsu.com"); // url to the nodejs / socket.io instance

            socket.Opened += SocketOpened;
            socket.Message += SocketMessage;
            socket.SocketConnectionClosed += SocketConnectionClosed;
            socket.Error += SocketError;

            // register for 'connect' event with io server
            socket.On("connect", (fn) =>
            {
                Console.WriteLine("\r\nConnected event...\r\n");
                socket.Send(new TextMessage("producer"));
                socket.Send(new TextMessage("UI-aston"));
                socket.Send(new TextMessage("SK-password"));
                socket.Send(new TextMessage("AT-"));

                socket.Send(new TextMessage("LV-0 Command From WPF App"));
                                
                connected = true;

            });


            // register for 'update' events - message is a json 'Part' object
            socket.On("update", (data) =>
            {
                Console.WriteLine("recv [socket].[update] event");
                //Console.WriteLine("  raw message:      {0}", data.RawMessage);
                //Console.WriteLine("  string message:   {0}", data.MessageText);
            });

            // make the socket.io connection
            socket.Connect();
        }        

       
        void SocketError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("socket client error:");
            Console.WriteLine(e.Message);
            currWSStatus = "Error" + e.Message;            
        }

        void SocketConnectionClosed(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocketConnection was terminated!");
            
            connected = false;
            messRec = false;
            errRec = false;
        }

        void SocketMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine("Generic SocketMessage: {0}", e.Message.MessageText);
            currWSMessage = "Message:" + e.Message.MessageText;                
            // uncomment to show any non-registered messages
            if (string.IsNullOrEmpty(e.Message.Event))
                Console.WriteLine("Generic SocketMessage: {0}", e.Message.MessageText);
            else
                Console.WriteLine("Generic SocketMessage: {0} : {1}", e.Message.Event, e.Message.Json.ToJsonString());
        }

        void SocketOpened(object sender, EventArgs e)
        {

        }

        public void CloseApp()
        {
            if (this.socket != null)
            {
                socket.Opened -= SocketOpened;
                socket.Message -= SocketMessage;
                socket.SocketConnectionClosed -= SocketConnectionClosed;
                socket.Error -= SocketError;
                this.socket.Dispose(); // close & dispose of socket client
            }
        }
        
        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (connected && socket.IsConnected)
            {               
               socket.Send(new TextMessage(txtMessage.Text));
            }
        }
        
        // to update controls
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            lblResponse.Content = currWSMessage;
            statusBarText.Text = currWSStatus;

            if (messRec)            
               statusBarText.Background = Brushes.DarkSeaGreen;            
             
            if (connected && socket.IsConnected)            
                statusBarText.Background = Brushes.DarkSeaGreen;
            else
                statusBarText.Background = Brushes.DarkGray;
                        
            if (errRec)            
                statusBarText.Background = Brushes.Red;            
            else
                statusBarText.Background = Brushes.DarkGray;
            

            if (connected && attemptConnect)
            {
                btnConnect.Content = "Connected";
                btnConnect.Background = Brushes.DarkSeaGreen;
                statusBarText.Text = "Connected";
                statusBarText.Background = Brushes.DarkSeaGreen;
            }
            else if (attemptConnect && !connected) 
            {
                btnConnect.Content = "Disconnected";
                btnConnect.Background = Brushes.DarkGray;
                statusBarText.Background = Brushes.DarkGray;
            }
        }

    }

}
