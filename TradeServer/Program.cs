using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace TradeServer
{

    // Класс-обработчик клиента
    public class Client
    {
        public static string LogFile = "Loggin.txt";
        object fileLock = new object();
        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public Client(TcpClient Client)
        {
            // Объявим строку, в которой будет хранится запрос клиента
            string Request = "";
            // Буфер для хранения принятых от клиента данных
            byte[] Buffer = new byte[1024];
            // Переменная для хранения количества байт, принятых от клиента
            int Count;
            // Читаем из потока клиента до тех пор, пока от него поступают данные
            while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
            {
                // Преобразуем эти данные в строку и добавим ее к переменной Request
                Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                // Запрос должен обрываться последовательностью \r\n\r\n
                // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
                // Нам не нужно получать данные из POST-запроса (и т. п.), а обычный запрос
                // по идее не должен быть больше 4 килобайт
                if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 1048576)
                {
                    break;
                }
            }

            lock(fileLock)
                File.AppendAllText(LogFile, Request);




            // Код простой HTML-странички
            string Html = $"<html><body><h1>It works! Time:{DateTime.Now}</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer1 = Encoding.ASCII.GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer1, 0, Buffer1.Length);
            // Закроем соединение
            Client.Close();
        }
    }

    public class Server
    {
        public static TcpListener Listener; // Объект, принимающий TCP-клиентов
               
        // Запуск сервера
        public Server(int Port)
        {
            File.AppendAllText(Client.LogFile, $"Start server - port:{Port}");
            // Создаем "слушателя" для указанного порта
            Listener = new TcpListener( IPAddress.Any, Port);
            Listener.Start(); // Запускаем его

            // В бесконечном цикле
            while (Listener != null)
            {
                try
                {
                    // Принимаем новых клиентов. После того, как клиент был принят, он передается в новый поток (ClientThread)
                    // с использованием пула потоков.
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listener.AcceptTcpClient());
                    // Принимаем новых клиентов и передаем их на обработку новому экземпляру класса Client
                    //new Client(Listener.AcceptTcpClient());
                } catch (Exception ex)
                {

                }

            }
        }

        // Остановка сервера
        ~Server()
        {
            Stop();
        }

        public void Stop()
        {
            // Если "слушатель" был создан
            if (Listener != null)
            {
                // Остановим его
                Listener.Stop();
                //Listener.EndAcceptSocket();
                //Listener.EndAcceptTcpClient();
                Listener = null;
            }
        }

        static void ClientThread(Object StateInfo)
        {
            new Client((TcpClient)StateInfo);
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Определим нужное максимальное количество потоков
            // Пусть будет по 4 на каждый процессор
            int MaxThreadsCount = Environment.ProcessorCount * 4;
            // Установим максимальное количество рабочих потоков
            ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
            // Установим минимальное количество рабочих потоков
            ThreadPool.SetMinThreads(3, 3);
            // Создадим новый сервер на порту 80

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
