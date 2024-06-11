using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using EI.SI;
using System.Threading;

namespace Server
{
    class Program
    {

        //variavel de desconectado o utilizador. Esta variavel fica a true quando o utilizador se desconectar
        private static bool user1disconect = false;
        private static bool user2disconect = false;

        private const int PORT = 10000;
        //lista de TCP Clientes para cada conecçao dos clientes. Esta lista serve para faze ro broadcast para todos os clientes
        private static List<TcpClient> tcpClientsList = new List<TcpClient>();

        private static int clientCounter = 0;
        // declaraçao do ProtocoloSI
        private static ProtocolSI protocolSI = new ProtocolSI();

        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            TcpListener listener = new TcpListener(endPoint);
            listener.Start();
            Console.WriteLine("Server is Ready!!!");
            // int clientCounter = 0;
            tcpClientsList.Clear();


            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                clientCounter++;
                Console.WriteLine("Client {0} connected", clientCounter);
                ClientHandler clientHandler = new ClientHandler(client, clientCounter);
                clientHandler.Handle();

            }


        }

        class ClientHandler
        {
            private TcpClient client;
            private int clientID;

            public ClientHandler(TcpClient client, int clientID)
            {
                //  this.client = client;
                // this.clientID = clientID;

                if (user1disconect == true)
                {
                    this.client = client;
                    this.clientID = 1;
                    user1disconect = false;
                }
                else
                {
                    this.client = client;
                    this.clientID = clientID;
                }
            }

            public void Handle()
            {
                Thread thread = new Thread(threadHandler);
                thread.Start();
            }

            private void threadHandler()
            {
                NetworkStream networkStream = this.client.GetStream();

                byte[] ack;
                //Adiciona o cliente a lista de clientes conectados
                if (clientID == 1)
                {
                    tcpClientsList.Insert(0, client);
                    user1disconect = false;
                }
                else if (clientID == 2)
                {
                    tcpClientsList.Insert(1, client);
                    //o cliente 2 ligou-se
                    user2disconect = true;
                }

                ack = protocolSI.Make(ProtocolSICmdType.ACK, clientCounter);
                networkStream.Write(ack, 0, ack.Length);

                while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
                {
                    int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);

                    switch (protocolSI.GetCmdType())
                    {
                        // caso o protocolo que o cliente enviar for USER_OPTION_1.
                        // Recebe a mensagem que o cliente que enviar para o adversário e envia para os oponentes
                        case ProtocolSICmdType.USER_OPTION_1:
                            //recebe a mensagem
                            string message = protocolSI.GetStringFromData();
                            string message_decifrada = "";
                            //decifra a mensagem consoante o Cliente
                            if (clientID == 1)
                            {

                                message_decifrada = message;
                            }
                            else if (clientID == 2)
                            {

                                message_decifrada = message;
                            }

                            // chama a função que envia para todos os clientes
                            BroadCast(message_decifrada, networkStream);
                            break;

                        case ProtocolSICmdType.PUBLIC_KEY:
                            Console.WriteLine("Client" + clientID + " : " + protocolSI.GetStringFromData());
                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);
                            break;


                        case ProtocolSICmdType.EOT:
                            Console.WriteLine("Ending Thread form cliente () " + clientID);
                            ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);
                            break;

                    }

                }


                networkStream.Close();
                client.Close();

            }

        }


        // função que envia para todos os clientes as mensagem de conversação que são enviadas para o servidor
        public static void BroadCast(string msg, NetworkStream networkStream)
        {
            string dadoscifradados = "";

            if (user1disconect == false)
            {
                dadoscifradados = msg;
                networkStream = tcpClientsList[0].GetStream();
                byte[] ack = protocolSI.Make(ProtocolSICmdType.MODE, dadoscifradados);
                networkStream.Write(ack, 0, ack.Length);
            }

            if (user2disconect == true)
            {
                dadoscifradados = msg;
                networkStream = tcpClientsList[1].GetStream();
                byte[] ack = protocolSI.Make(ProtocolSICmdType.MODE, dadoscifradados);
                networkStream.Write(ack, 0, ack.Length);
            }

        }


    }
}
