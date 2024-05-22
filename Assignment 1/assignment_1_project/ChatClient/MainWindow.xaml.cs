using System;
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
using ChatServerInterface;
using System.ServiceModel;
using DLL;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Drawing;
using System.Windows.Interop;

namespace ChatClient
{
    public partial class MainWindow : Window, IChatCallback
    {
        private IChatService service;
        private User user;
        private static ObservableCollection<string> chatRoomList = new ObservableCollection<string>();
        private ObservableCollection<string> chatRoomParticipantList;
        private string currRoom;
        private string selectedFilePath;
        private string selectedFilename;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InstanceContext context = new InstanceContext(this);
                NetTcpBinding netTcpBinding = new NetTcpBinding();
                netTcpBinding.MaxReceivedMessageSize = 200_000_000;
                DuplexChannelFactory<IChatService> factory = new DuplexChannelFactory<IChatService>(context, netTcpBinding, "net.tcp://localhost:8000/ChatService");
                service = factory.CreateChannel();

                string username = UsernameTextBox.Text;

                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new Exception("Username cannot be empty!");
                }

                user = new User(username);

                service.ConnectUser(user);

                Title = $"Chat Room - {username}";

                MessageTextBox.IsEnabled = true;
                SendBtn.IsEnabled = true;

                chatRoomList = service.getChatRooms();
                
                RoomsDDM.ItemsSource = chatRoomList;

                LoginPanel.Visibility = Visibility.Collapsed;
                ChatPanel.Visibility = Visibility.Visible;
                TextPanel.Visibility = Visibility.Collapsed;
            }
            catch (FaultException<ServerFault> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /*
         * Callback method from IChatCallback
         */
        public void ReceiveMessage(Message message)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run($"{message.Time.TimeOfDay.Hours}:{message.Time.TimeOfDay.Minutes} {message.From}: {message.Text}\n"));
            if (message.Attachemnt != null)
            {
                //MemoryStream memoryStream = new MemoryStream(message.Attachemnt);
                //Bitmap bitmap = new Bitmap(memoryStream);
                //System.Windows.Controls.Image image = new System.Windows.Controls.Image();

                //BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                //image.Source = bitmapSource;

                //InlineUIContainer inlineUIContainer = new InlineUIContainer(image);
                //paragraph.Inlines.Add(inlineUIContainer);

                Hyperlink hyperlink = new Hyperlink();
                hyperlink.TextDecorations = TextDecorations.Underline;

                string imageStorageDirectory = Directory.GetCurrentDirectory() + "\\images";

                if (!Directory.Exists(imageStorageDirectory))
                {
                    Directory.CreateDirectory(imageStorageDirectory);
                }
                string filepath = Directory.GetCurrentDirectory() + $"\\images\\{DateTime.Now:yyyyMMddHHmmssfff}" + message.Filename;

                File.WriteAllBytes(filepath, message.Attachemnt);

                hyperlink.Inlines.Add(new Run(filepath));
                hyperlink.NavigateUri = new Uri(filepath);
                hyperlink.RequestNavigate += OpenFileFromHyperLink;

                paragraph.Inlines.Add(hyperlink);
            }
            ChatTextBox.Document.Blocks.Add(paragraph);
        }


        /*
         * Navigation method added to hyperlink to open file when the filepath is clicked.
         */
        private void OpenFileFromHyperLink(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Uri uri = new Uri(e.Uri.ToString());
                System.Diagnostics.Process.Start(uri.LocalPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}");
            }
        }


        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string to = UsersDDM.SelectedItem.ToString();
                string text = MessageTextBox.Text;


                if (!string.IsNullOrWhiteSpace(text) || selectedFilePath != null)
                {
                    Message message = new Message(text, user.Username, to);
                    if (selectedFilePath != null)
                    {
                        message.Attachemnt = File.ReadAllBytes(selectedFilePath);
                        message.Filename = selectedFilename;
                    }

                    await Task.Run(() =>
                    {
                        service.SendMessage(currRoom, message);
                    });
                }

                MessageTextBox.Clear();
                SelectedFilePathLabel.Content = "Selected file : ";
                selectedFilePath = null;
                selectedFilename = null;


            } catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void LogOutBtn_Click(object sender, RoutedEventArgs e)
        {
            service.DisconnectUser(user);
            LoginPanel.Visibility = Visibility.Visible;
            ChatPanel.Visibility = Visibility.Collapsed;
        }

        private void JoinBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currRoom != null)
                {
                    service.ExitChatRoom(currRoom, user);
                }

                Object selectedRoom = RoomsDDM.SelectedItem;

                if (selectedRoom != null)
                {
                    currRoom = selectedRoom.ToString();
                    Task.Run(() =>
                    {
                        service.JoinChatRoom(currRoom, user);
                    });

                    ChatRoomLbl.Content = currRoom;

                    ChatTextBox.Document.Blocks.Clear();

                    chatRoomParticipantList = new ObservableCollection<string>(service.getParticipants(currRoom).Where(p => p != user.Username).ToList());
                    UsersDDM.ItemsSource = chatRoomParticipantList;
                    UsersDDM.SelectedIndex = 0;

                    TextPanel.Visibility = Visibility.Visible;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void CreatBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newRoomName = CreateTextBox.Text;

                if (string.IsNullOrWhiteSpace(newRoomName))
                {
                    throw new Exception("ChatRoom Name cannot be empty!");
                }

                Task.Run(() =>
                {
                    service.CreateChatRoom(newRoomName);

                });
                CreateTextBox.Clear();
            }
            catch (FaultException<ServerFault> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void RefreashBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UsersDDM.Dispatcher.Invoke((Action)(() =>
                {
                    UsersDDM.ItemsSource = service.getParticipants(currRoom).Where(p => p != user.Username).ToList();
                    UsersDDM.SelectedIndex = 0;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SelectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select a file to upload";

                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    selectedFilePath = openFileDialog.FileName;
                    selectedFilename = System.IO.Path.GetFileName(selectedFilePath);
                    SelectedFilePathLabel.Content = $"Selected file : {selectedFilename}";
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }



        /*
         * Callback method from IChatCallback
         */
        public void UpdateChatRoomInfo(string chatRoomName)
        {
           chatRoomList.Add(chatRoomName);
        }

    }
}
