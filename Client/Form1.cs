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

namespace Projeto_Topicos_de_Segurança
{
    public partial class Form1 : Form
    {
        private const int PORT = 10000;
        private string IP = IPAddress.Loopback.ToString();
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        TcpClient client;
        IPEndPoint endPoint;

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
            catch (Exception)
            {
                MessageBox.Show("Erro no envio de dados ao servidor.", "Error Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
