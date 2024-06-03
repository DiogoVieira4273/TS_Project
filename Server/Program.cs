using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using EI.SI;
using System.Threading;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.IO;

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
        private const int SALTSIZE = 8;
        private const int NUMBER_OF_ITERATIONS = 1000;
        RSACryptoServiceProvider rsa;
        AesCryptoServiceProvider aes;

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

        private bool VerifyLogin(string username, string password)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();
                conn.ConnectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\diogo\Desktop\TESP PSI\2023_2024\2º Semestre\TS\Projeto\TS_Project\Server\Projeto.mdf;Integrated Security=True");

                // Abrir ligação à Base de Dados
                conn.Open();

                // Declaração do comando SQL
                string sql = "SELECT * FROM Users WHERE Username = @username";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;

                // Declaração dos parâmetros do comando SQL
                SqlParameter param = new SqlParameter("@username", username);

                // Introduzir valor ao parâmentro registado no comando SQL
                cmd.Parameters.Add(param);

                // Associar ligação à Base de Dados ao comando a ser executado
                cmd.Connection = conn;

                // Executar comando SQL
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    throw new Exception("Error while trying to access an user");
                }

                // Ler resultado da pesquisa
                reader.Read();

                // Obter Hash (password + salt)
                byte[] saltedPasswordHashStored = (byte[])reader["SaltedPasswordHash"];

                // Obter salt
                byte[] saltStored = (byte[])reader["Salt"];

                conn.Close();

                byte[] hash = GenerateSaltedHash(password, saltStored);

                return saltedPasswordHashStored.SequenceEqual(hash);

                //TODO: verificar se a password na base de dados 
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return false;
            }
        }

        private void Register(string username, byte[] salt, byte[] saltedPasswordHash)
        {
            SqlConnection conn = null;
            try
            {
                // Configurar ligação à Base de Dados
                conn = new SqlConnection();
                conn.ConnectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\diogo\Desktop\TESP PSI\2023_2024\2º Semestre\TS\Projeto\TS_Project\Server\Projeto.mdf;Integrated Security=True");

                // Abrir ligação à Base de Dados
                conn.Open();

                // Declaração dos parâmetros do comando SQL
                SqlParameter paramUsername = new SqlParameter("@username", username);
                SqlParameter paramSalt = new SqlParameter("@salt", salt);
                SqlParameter paramPassHash = new SqlParameter("@saltedPasswordHash", saltedPasswordHash);

                // Declaração do comando SQL
                string sql = "INSERT INTO Users (Username, SaltedPasswordHash, Salt) VALUES (@username,@saltedPasswordHash,@salt)";

                // Prepara comando SQL para ser executado na Base de Dados
                SqlCommand cmd = new SqlCommand(sql, conn);

                // Introduzir valores aos parâmentros registados no comando SQL
                cmd.Parameters.Add(paramUsername);
                cmd.Parameters.Add(paramPassHash);
                cmd.Parameters.Add(paramSalt);

                // Executar comando SQL
                int lines = cmd.ExecuteNonQuery();

                // Fechar ligação
                conn.Close();

                if (lines == 0)
                {
                    // Se forem devolvidas 0 linhas alteradas então o não foi executado com sucesso
                    throw new Exception("Error while inserting an user");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error while inserting an user:" + e.Message);
            }
        }

        private static byte[] GenerateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return buff;
        }

        private static byte[] GenerateSaltedHash(string plainText, byte[] salt)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(plainText, salt, NUMBER_OF_ITERATIONS);
            return rfc2898.GetBytes(32);
        }

        //GERAR UMA CHAVE SIMÉTRICA A PARTIR DE UMA STRING
        private string GerarChavePrivada(string pass)
        {
            // O salt tem de ter no mínimo 8 bytes e não
            // é mais do que array be bytes. O array é caracterizado pelo []
            byte[] salt = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(pass, salt, 1000);

            //GERAR KEY
            byte[] key = pwdGen.GetBytes(16);
            string passB64 = Convert.ToBase64String(key);

            return passB64;
        }

        //GERAR UM VETOR DE INICIALIZAÇÃO A PARTIR DE UMA STRING
        private string GerarIV(string pass)
        {
            byte[] salt = new byte[] { 7, 6, 5, 4, 3, 2, 1, 0 };

            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(pass, salt, 1000);

            //GERAR IV
            byte[] iv = pwdGen.GetBytes(16);

            string ivB64 = Convert.ToBase64String(iv);

            return ivB64;
        }

        //MÉTODO PARA CIFRAR O TEXTO
        private string CifrarTexto(string txt)
        {

            //VARIÁVEL PARA GUARDAR O TEXTO DECIFRADO EM BYTES
            byte[] txtDecifrado = Encoding.UTF8.GetBytes(txt);

            //VARIÁVEL PARA GUARDAR O TEXTO CIFRADO EM BYTES
            byte[] txtCifrado;

            //RESERVAR ESPAÇO NA MEMÓRIA PARA COLOCAR O TEXTO E CIFRÁ-LO
            MemoryStream ms = new MemoryStream();

            //INICIALIZAR O SISTEMA DE CIFRAGEM (WRITE)
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);

            //CIFRAR OS DADOS
            cs.Write(txtDecifrado, 0, txtDecifrado.Length);
            cs.Close();

            //GUARDAR OS DADOS CIFRADO QUE ESTÃO NA MEMÓRIA
            txtCifrado = ms.ToArray();

            //CONVERTER OS BYTES PARA BASE64 (TEXTO)
            string txtCifradoB64 = Convert.ToBase64String(txtCifrado);

            //DEVOLVER OS BYTES CRIADOS EM BASE64
            return txtCifradoB64;
        }


        //MÉTODO PARA DECIFRAR O TEXTO
        private string DecifrarTexto(string txtCifradoB64)
        {
            //VARIÁVEL PARA GUARDAR O TEXTO CIFRADO EM BYTES
            byte[] txtCifrado = Convert.FromBase64String(txtCifradoB64);

            //RESERVAR ESPAÇO NA MEMÓRIA PARA COLOCAR O TEXTO E DECIFRÁ-LO
            MemoryStream ms = new MemoryStream(txtCifrado);

            //INICIALIZAR O SISTEMA DE CIFRAGEM (READ)
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);

            //VARIÁVEL PARA GUARDAR O TEXTO DECIFRADO
            byte[] txtDecifrado = new byte[ms.Length];

            //VARIÁVEL PARA TER O NÚMERO DE BYTES DECIFRADOS
            int bytesLidos = 0;

            //DECIFRAR OS DADOS
            bytesLidos = cs.Read(txtDecifrado, 0, txtDecifrado.Length);
            cs.Close();

            //CONVERTER PARA TEXTO
            string textoDecifrado = Encoding.UTF8.GetString(txtDecifrado, 0, bytesLidos);

            //DEVOLVER TEXTO DECIFRADO
            return textoDecifrado;
        }
    }
}
