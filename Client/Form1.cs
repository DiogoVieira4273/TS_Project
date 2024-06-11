using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using EI.SI;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Remoting.Lifetime;
using System.Net.NetworkInformation;

namespace Client
{
    public partial class Form1 : Form
    {
        private const int PORT = 10000;
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        TcpClient client;
        IPEndPoint endPoint;
        RSACryptoServiceProvider rsa;
        RSACryptoServiceProvider rsaVerify;
        AesCryptoServiceProvider aes;
        private const int SALTSIZE = 8;
        private const int NUMBER_OF_ITERATIONS = 1000;

        public Form1()
        {
            try
            {
                // cria a ligação com o cliente
                endPoint = new IPEndPoint(IPAddress.Loopback, PORT);
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

       

        //GERAR UMA CHAVE SIMÉTRICA A PARTIR DE UMA STRING
        private string GerarChavePrivada()
        {
            // O salt tem de ter no mínimo 8 bytes e não
            // é mais do que array be bytes. O array é caracterizado pelo []
            string pass = "TS";
            byte[] salt = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(pass, salt, 1000);

            //GERAR KEY
            byte[] key = pwdGen.GetBytes(16);
            string passB64 = Convert.ToBase64String(key);

            return passB64;
        }

        //GERAR UM VETOR DE INICIALIZAÇÃO A PARTIR DE UMA STRING
        private string GerarIV()
        {
            string pass = "TS";

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

        private void buttonEnviarMensagem_Click(object sender, EventArgs e)
        {
            rsa = new RSACryptoServiceProvider();
            aes = new AesCryptoServiceProvider();
            rsaVerify = new RSACryptoServiceProvider();

            string privateKeyFile = "../../../privatekey.txt";
            string ivFile = "../../../IV.txt";
            string publicKeyFile = "../../../publickey.txt";
            string bothFile = "../../../bothkeys.txt";
            string hFile = "../../../hash.txt";
            string signFile = "../../../signature.txt";

            byte[] data;
            byte[] hash;

            string publicKey = rsa.ToXmlString(false);
            File.WriteAllText(publicKeyFile, publicKey);

            string bothKeys = rsa.ToXmlString(true);
            File.WriteAllText(bothFile, bothKeys);

            string privateKey = GerarChavePrivada();
            string IV = GerarIV();
            File.WriteAllText(privateKeyFile, privateKey);
            File.WriteAllText(ivFile, IV);

            byte[] keyaes = Convert.FromBase64String(privateKey);
            aes.Key = keyaes;

            byte[] ivaes = Convert.FromBase64String(IV);
            aes.IV = ivaes;

            string msg = textBoxEscreverMensagem.Text;
            rsaVerify.FromXmlString(publicKey);

            if (msg == "")
            {
                MessageBox.Show("Escrever mensagem", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                textBoxEscreverMensagem.Clear();

                string msgEnc = CifrarTexto(textBoxEscreverMensagem.Text);
                byte[] meb = Convert.FromBase64String(msgEnc);

                using (SHA1 sha1 = SHA1.Create())
                {
                    data = Encoding.UTF8.GetBytes(msg);

                    hash = sha1.ComputeHash(data);
                }
                File.WriteAllBytes(hFile, hash);

                byte[] sign = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
                File.WriteAllBytes(signFile, sign);

                byte[] encrypt = rsa.Encrypt(meb, RSAEncryptionPadding.Pkcs1);

                byte[] pack = protocolSI.Make(ProtocolSICmdType.DATA, encrypt);
                networkStream.Write(pack, 0, pack.Length);
            }
            listBoxConversa.Items.Add(msg);
        }

        private void button_Login_Click(object sender, EventArgs e)
        {
            string user = textBoxUsername.Text;
            string pass = textBoxPassword.Text;


            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Username or password can't be blank");
            }
            else
            {
                if (UsernameExists(user))
                {
                    textBoxEscreverMensagem.Enabled = true;
                    buttonEnviarMensagem.Enabled = true;
                }
                //var juntar = Combinar(user, pass);
                //EnviarLogin(juntar);
            }
        }

        private bool UsernameExists(string username)
        {
            using (SqlConnection connection = new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;AttachDbFilename=C:\Users\diogo\Desktop\TESP_PSI\2023_2024\2ºSemestre\TS\Projeto\TS_Project\Client\Projeto.mdf;Integrated Security=True"))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    int userCount = (int)command.ExecuteScalar();

                    return userCount > 0;
                }
            }
        }

        private string Combinar(string user, string pass)
        {
            string juntar = (user + "+" + pass);

            return juntar;
        }

        private void EnviarLogin(string juntar)
        {
            try
            {
                byte[] senhaBytes = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, juntar);

                networkStream.Write(senhaBytes, 0, senhaBytes.Length);

                while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
                {
                    MessageBox.Show("Login recept");

                    switch (protocolSI.GetCmdType())
                    {
                        case ProtocolSICmdType.USER_OPTION_3:
                            var msg = protocolSI.GetStringFromData();
                            var login = Convert.ToBoolean(msg);

                            if (login == true)
                            {
                                MessageBox.Show("Login succeed");
                                textBoxUsername.Clear();
                                textBoxPassword.Clear();
                                textBoxEscreverMensagem.Enabled = true;
                                buttonEnviarMensagem.Enabled = true;
                            }
                            else if (login == false)
                            {
                                MessageBox.Show("Login error");
                            }
                            break;

                        default:
                            MessageBox.Show("Login again");
                            break;
                    }
                    return;
                }
                networkStream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_Registo_Click(object sender, EventArgs e)
        {
            string user = textBoxUsername.Text;
            string pass = textBoxPassword.Text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Username or password can't be blank");
            }
            else if (UsernameExists(user))
            {
                MessageBox.Show("Username already exists");
            }
            else if (user != "miguel" && user != "diogo")
            {
                MessageBox.Show("Username must be 'miguel' or 'diogo'");
            }
            else
            {
                var combinar = Combinar(user, pass);
                RegistarLogin(combinar);
            }
        }

        public static byte[] GenerateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return buff;
        }

        public static byte[] GenerateSaltedHash(string plainText, byte[] salt)
        {
            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(plainText, salt, NUMBER_OF_ITERATIONS);
            return rfc2898.GetBytes(32);
        }

        private void RegistarLogin(string cominar)
        {
            try
            {
                byte[] senhaByes = protocolSI.Make(ProtocolSICmdType.USER_OPTION_2, cominar);

                networkStream.Write(senhaByes, 0, senhaByes.Length);

                while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
                {
                    MessageBox.Show("Register recept");

                    switch (protocolSI.GetCmdType())
                    {
                        case ProtocolSICmdType.USER_OPTION_4:
                            var msgs = protocolSI.GetStringFromData();
                            var reg = Convert.ToBoolean(msgs);
                            if (reg == true)
                            {
                                MessageBox.Show("Register succeed");
                                textBoxUsername.Clear();
                                textBoxPassword.Clear();
                            }
                            else if (reg == false)
                            {
                                MessageBox.Show("Register error");
                            }
                            break;
                        default:
                            MessageBox.Show("Register again");
                            break;
                    }
                    return;
                }
                networkStream.Close();
                client.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}