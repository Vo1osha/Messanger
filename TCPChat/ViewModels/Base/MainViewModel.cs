using Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TCPChat.Infrastructure;
using TCPChat.Infrastructure.Commands;

namespace TCPChat.ViewModels
{
    //
    class MainViewModel : ViewModel 
    {
        /// <summary>c-ва для Освобождение каких либо инструкций ( не знаю пригодится ли) </summary>
        // protected override void Dispose(bool Disposing) {base.Dispose(Disposing); }
        #region Заголовок окна 
        /// <summary>Заголовок окна<summary>
        // Код для добавления названий и их реал лайф изменения с помощью OnPropertyChanged "может быть пригодится в будущем"

        private string _Title = "Мессенджер ВНИИРТ";
        public string Title
        {
            get => _Title;
            //set
            //{
            //     if (Equals(_Title, value)) return;
            //    _Title = value;
            //      OnPropertyChanged();
            //    }
            //  }

            // упрощенная генерация

          //!!!!  set => Set(ref _Title, value); !!!!!

            // упрощенная генерация 
        }
        // В каждом свойстве должен быть код,если нужно изменить текст в нем!
        #endregion
        /// <summary>
        /// вижен на отправление сообщений + текста + юзеров
        /// </summary>
        #region Интерфейс
        private Visibility _connectIsVisible;
        public Visibility ConnectIsVisible
        {
            set
            {
                _connectIsVisible = value;
                OnPropertyChanged();
            }
            get => _connectIsVisible;
        }

        private Visibility _warningVisibility;
        public Visibility WarningVisibility
        {
            set
            {
                _warningVisibility = value;
                OnPropertyChanged();
            }
            get => _warningVisibility;
        }

        private bool _sendIsEnable;
        public bool SendIsEnable
        {
            set
            {
                _sendIsEnable = value;
                OnPropertyChanged();
            }
            get => _sendIsEnable;
        }

        private Visibility _usernameTakenLabelIsEnable;
        public Visibility UsernameTakenLabelIsEnable
        {
            set
            {
                _usernameTakenLabelIsEnable = value;
                OnPropertyChanged();
            }
            get => _usernameTakenLabelIsEnable;
        }

        private string _textMessage;
        public string TextMessage
        {
            set
            {
                _textMessage = value;
                OnPropertyChanged();
            }
            get => _textMessage;
        }

        private string _receiver;
        public string Receiver
        {
            set
            {
                _receiver = value;
                OnPropertyChanged();
            }
            get => _receiver;
        }

        #endregion

        #region Команды
        public ICommand ConnectCommand { set; get; }
        public ICommand SendCommand { set; get; }
        public ICommand SendEmailCommand { set; get; }
        #endregion

        #region Переменные
        private User user;
        private IPEndPoint ep;
        private NetworkStream nwStream;

        private string selectedUser;
        private string username;
        #endregion
        
        #region Данные Для сервера
        public ObservableCollection<MessageUI> MessagessItems { set; get; }

        public string SelectedUser
        {
            set
            {
                selectedUser = value;
                if (value == Users.ElementAt(0))
                    Receiver = Users.ElementAt(0).ToLower();
                else
                    Receiver = $"только {value}";

                OnPropertyChanged();
            }
            get => selectedUser;
        }

        public string Username
        {
            set
            {
                username = value;
                OnPropertyChanged();
            }
            get => username;
        }

        public ObservableCollection<string> Users { set; get; }
        public string EmailAddress { set; get; }
        #endregion
       // test public string Users1 = "User";
        public MainViewModel()
        {
            ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23016);
            user = new User();
            
             Users = new ObservableCollection<string>();
             Users.Add("Всем"); 
             SelectedUser = Users.ElementAt(0);
                
            ConnectIsVisible = Visibility.Visible;
            WarningVisibility = UsernameTakenLabelIsEnable = Visibility.Hidden;
            MessagessItems = new ObservableCollection<MessageUI>();
            InitCommands();

            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
            CloseapplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecute, CanCloseApplicationExecute);
        }
        #region Тесты
        //testing commands tak to
        public ICommand ConnectingCOmmand { get;}
        private bool CanConnect(object p) => true;

        private void OnConnect(object p)
        {

            InitCommands();
        }
        #endregion
        #region CloseappCommand
        public ICommand CloseapplicationCommand { get; }

        private bool CanCloseApplicationExecute(object p) => true;

        private void OnCloseApplicationCommandExecute(object p)
        {
            try
            {
                SendData(new Message { Sender = user, ServerMessage = ServerMessage.RemoveUser });
                Application.Current.Shutdown();
            }
            catch {  }
           

        }
        #endregion

        #region Функции работы с сервером
        private void InitCommands()
        {
            ConnectCommand = new RelayCommand(x => Task.Run(() =>
            {
                if (!Connect())
                {
                    UsernameTakenLabelIsEnable = Visibility.Visible;
                    return;
                }

                ConnectIsVisible = UsernameTakenLabelIsEnable = Visibility.Hidden;
                WarningVisibility = Visibility.Visible;
                SendIsEnable = true;
                user.Username = Username;

                GetData();
            }));

            SendCommand = new RelayCommand(x => Task.Run(() =>
            {
                Message message = new Message()
                {
  
                    MessageString = TextMessage,
                    Sender = user
                };

                if (Users.ElementAt(0) == SelectedUser || SelectedUser == null)
                    message.ServerMessage = ServerMessage.Broadcast;
                else
                {
                    message.ServerMessage = ServerMessage.Message;
                    message.Reciever = new User() { Username = SelectedUser };
                }
                SendData(message);
                TextMessage = string.Empty;
            }
            ));
        }


        public bool Connect()
        {

            user.Client = new TcpClient();
            user.Client.Connect(ep);
            nwStream = user.Client.GetStream();

             Username = Username ?? "Неизвестный";
            // Username = Environment.UserName ?? "Неизвестный";
            nwStream.Write(Encoding.Default.GetBytes(Username), 0, Username.Length);
            nwStream.Flush();

            BinaryFormatter bf = new BinaryFormatter();
            Message message = (Message)bf.Deserialize(nwStream);
            if (message.ServerMessage == ServerMessage.WrongUsername)
            {
                user.Client.Client.Shutdown(SocketShutdown.Both);
                user.Client.Close();
                return false;
            }
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var i in message.Users)
                    Users.Add(i.Username);
            }));

            return true;
        }

        public void GetData()
        {
            BinaryFormatter bf = new BinaryFormatter();
            while (true)
            {
                try
                {
                    Message message = (Message)bf.Deserialize(nwStream);
                    if (message.ServerMessage == ServerMessage.Message)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (message.Sender.Username == Username)
                                MessagessItems.Add(new MessageUI() { Sender = $"Я: ", Message = message.MessageString, Color = "#000000", FontStyle = FontStyles.Normal, Align = "Right" });
                            else
                                MessagessItems.Add(new MessageUI() { Sender = $"{message.Sender.Username}: ", Message = message.MessageString, Color = "#000000", FontStyle = FontStyles.Normal, Align = "Left" });
                        }));
                    }
                    else if (message.ServerMessage == ServerMessage.AddUser)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            MessagessItems.Add(new MessageUI() { Sender = message.Sender.Username, Message = " Зашел в чат", Color = "#40698c", FontStyle = FontStyles.Oblique, Align = "Left" });
                            if (!Users.Contains(message.Sender.Username))
                                Users.Add($"{message.Sender.Username}");
                        }));
                    }
                    else if (message.ServerMessage == ServerMessage.RemoveUser)
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            MessagessItems.Add(new MessageUI() { Sender = message.Sender.Username, Message = "Вышел из чата", Color = "#40698c", FontStyle = FontStyles.Oblique, Align = "Left" });
                            Users.Remove(Users.Where(x => x == message.Sender.Username).First());
                        }));
                    }
                }

                catch (Exception e) { Console.WriteLine(e.Message); }

            }
        }

        public void SendData(Message message)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(nwStream, message);
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                SendData(new Message { Sender = user, ServerMessage = ServerMessage.RemoveUser });
            }
            catch { }
        }
    #endregion
     }   
}
