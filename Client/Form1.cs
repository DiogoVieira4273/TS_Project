using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using EI.SI;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace Client
{
    public partial class Form1 : Form
    {
        private const int PORT = 10000;
        private string IP = IPAddress.Loopback.ToString();
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        TcpClient client;
        IPEndPoint endPoint;
        private const int SALTSIZE = 8;
        private const int NUMBER_OF_ITERATIONS = 1000;  

        public Form1()
        {
            try
            {
                // cria a ligação com o cliente
                endPoint = new IPEndPoint(IPAddress.Parse(IP), PORT);
                client = new TcpClient();
                client.Connect(endPoint);
                networkStream = client.GetStream();
                protocolSI = new ProtocolSI();

                // verifica se a ligação do cliente com o servidor foi bem sucessida
                //se for true a ligação foi bem sucedida, se for falso a ligação nao foi bem sucedida porque o servidor já esta lotado
                if (Ligacao(protocolSI) == true)
                {
                    //conecção com o servidor autenticado
                    InitializeComponent();
                    // inicia a thread do cliente para ficar a escuta dos dados que o servidor enviar
                    Thread thread = new Thread(threadClient);
                    thread.Start(client);


                }
                else
                {
                    // conecção com o servidor rejeitada
                    InitializeComponent();
                    listBoxConversa.Items.Add(" Server - Numero maximo de clientes");
                    //Select_item_listbox();
                    //connected = false;
                    byte[] eot = protocolSI.Make(ProtocolSICmdType.ACK);
                    networkStream.Write(eot, 0, eot.Length);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error. Ligação com o servidor", "Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                InitializeComponent();
                //errorconnected = true;
            }
        }

        private RSACryptoServiceProvider rsa;
        private AesCryptoServiceProvider aes;

        // thread cliente que recebe dados do servidor a partir de tipos de protocolos especificos que o servidor envia mensagem desse tipo
        private void threadClient(object obj)
        {
            byte[] ack;
            try
            {
                while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
                {
                    int bytesread = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    string resposta = "";
                    string decifrar_resposta = "";
                    switch (protocolSI.GetCmdType())
                    {
                        //caso o protocolo seja do tipo Mode escreve na listBox_chat a mensagem dos clientes
                        case ProtocolSICmdType.MODE:
                            string dados_mensagem = protocolSI.GetStringFromData();
                            string dados_mensagem_decifrados = "";
                            /// dados_mensagem_decifrados = DecifrarTexto(dados_mensagem);
                            dados_mensagem_decifrados = dados_mensagem;
                            string[] itemsmessage = dados_mensagem_decifrados.Split(';');
                            this.Invoke((MethodInvoker)delegate
                            {
                                listBoxConversa.Items.Add(itemsmessage[0]);
                                //     listBoxConversa.Text = dados_mensagem_decifrados;

                            });
                            //Select_item_listbox();
                            break;

                        // caso o protoloco for do tipo DATA.
                        // Este protocolo serve para atualizar a célula de jogo no tabuleiro que foi selecionada pelo cliente
                        case ProtocolSICmdType.DATA:
                            resposta = protocolSI.GetStringFromData();
                            // decifrar_resposta = DecifrarTexto(resposta);
                            decifrar_resposta = resposta;
                            string[] itemsposicao = decifrar_resposta.Split(';');
                            int[] itemsposicaoint = new int[2];
                            break;

                        // caso o protocolo for EOT é porque o cliente quer se desconectar
                        case ProtocolSICmdType.EOT:
                            try
                            {
                                // enquanto o servidor não retornar com diferente de EOT não dá ordem para o clietne terminar
                                while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
                                {
                                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                                }
                            }
                            catch (Exception)
                            {
                            }
                            break;
                    }
                }

                while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
                {
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                }
                ack = protocolSI.Make(ProtocolSICmdType.ACK);
                networkStream.Write(ack, 0, ack.Length);
                // desliga a coneção e o networkstream
                client.Close();
                networkStream.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Erro com o servidor. Problemas técnicos", "Error Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Invoke((MethodInvoker)delegate
                {
                    //groupBox1.Enabled = false;
                    //  groupBox3.Enabled = false;
                });
            }
        }

        //funçao que verificca se a conecção com o servidor é válida
        // retorna false se o servidor estiver completo e true se o servidor aceitar clientes
        private bool Ligacao(ProtocolSI protocol)
        {
            try
            {
                while (protocol.GetCmdType() != ProtocolSICmdType.ACK)
                {
                    networkStream.Read(protocol.Buffer, 0, protocol.Buffer.Length);
                    if (protocol.GetCmdType() == ProtocolSICmdType.EOT)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Erro na comunicação com o servidor.", "Error Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void Form1_Load(object sender, EventArgs e) //Não é necessário
        {

        }

        private void buttonEnviarMensagem_Click(object sender, EventArgs e)
        {

            string dadoscifradados = textBoxEscreverMensagem.Text;
            try
            {
                //criação de um array de bytes com a mensagem a enviar e envio da mesma para o servidor

                byte[] packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, dadoscifradados);
                // Enviar mensagem
                networkStream.Write(packet, 0, packet.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                textBoxEscreverMensagem.Text = "";
            }
        }

        private void CloseClient()
        {
            byte[] eot = protocolSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(eot, 0, eot.Length);
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            networkStream.Close();
            client.Close();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            CloseClient();
            this.Close();
        }

        private void button_Registo_Click(object sender, EventArgs e)
        {
            string user = textBoxUsername.Text;
            string pass = textBoxPassword.Text;

            byte[] salt = GenerateSalt(SALTSIZE);
            byte[] saltedPasswordHash = GenerateSaltedHash(pass, salt);

            if (string.IsNullOrEmpty(textBoxUsername.Text) || string.IsNullOrEmpty(textBoxPassword.Text))
            {
                MessageBox.Show("Username or password can't be blank");
            }

            Register(user, salt, saltedPasswordHash);
        }

        private void button_Login_Click(object sender, EventArgs e)
        {
            string user = textBoxUsername.Text;
            string pass = textBoxPassword.Text;

            if (VerifyLogin(user, pass))
            {
                MessageBox.Show("Utilizador logado com sucesso", "Início de sessão com sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Erro de inicio de sessão", "Utilizador ou palavara-passe incorretos", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("An error occurred: " + e.Message);
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

        private string GerarChavePrivada(string pass)
        {
            byte[] salt = new byte[] { 1, 9, 3, 4, 1, 0, 5, 8 }; // this is fixed... It would be better you used something different for each user

            // You can raise 1000 to greater numbers... more cycles = more security. Try
            // balancing speed with security.
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(pass, salt, 1000);

            //generate key
            byte[] key = pwdGen.GetBytes(16);

            //CONVERTER A PASS EM BASE64
            string passB64 = Convert.ToBase64String(key);

            //DEVOLVER A PASS EM BYTES
            return passB64;
        }

        private void GenerateChavePublica()
        {
            rsa = new RSACryptoServiceProvider();

            string publicKey = rsa.ToXmlString(false);
        }

        private void GenerateChaveSimetrica()
        {
            rsa = new RSACryptoServiceProvider();

            string bothKey = rsa.ToXmlString(true);
        }

        
    }
}
