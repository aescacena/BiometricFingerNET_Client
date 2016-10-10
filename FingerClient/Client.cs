using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FingerClient
{
    public delegate void ClientHandlePacketData(byte[] data, int bytesRead);

    /// <summary>
    /// Implements a simple TCP client which connects to a specified server and
    /// raises C# events when data is received from the server
    /// </summary>
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream clientStream;
        private NetworkBuffer buffer;
        private int writeBufferSize = 1024;
        private int readBufferSize = 1024;
        private int port;
        private bool started = false;
        public int estadoHuella = 0;

        /// <summary>
        /// Constructs a new client
        /// </summary>
        public Client()
        {
            estadoHuella = 0;
            buffer = new NetworkBuffer();
            buffer.WriteBuffer = new byte[writeBufferSize];
            buffer.ReadBuffer = new byte[readBufferSize];
            buffer.CurrentWriteByteCount = 0;
        }

        /// <summary>
        /// Initiates a TCP connection to a TCP server with a given address and port
        /// </summary>
        /// <param name="ipAddress">The IP address (IPV4) of the server</param>
        /// <param name="port">The port the server is listening on</param>
        public void ConnectToServer(string ipAddress, int port)
        {
            this.port = port;

            tcpClient = new TcpClient(ipAddress, port);
            clientStream = tcpClient.GetStream();
            Console.WriteLine("Connected to server, listening for packets");

            Thread t = new Thread(new ThreadStart(ListenForPackets));
            started = true;
            t.Start();
        }

        /// <summary>
        /// This method runs on its own thread, and is responsible for
        /// receiving data from the server and raising an event when data
        /// is received
        /// </summary>
        private void ListenForPackets()
        {
            int bytesRead;
            string estado = "INICIAL";
            ComunicacionStream cS = new ComunicacionStream(tcpClient.GetStream());
            while (started)
            {
                bytesRead = 0;

                try
                {
                    switch (estado)
                    {
                        case "INICIAL":
                            //bloquea hasta que un cliente envía imagen de dedo
                            Console.WriteLine("Envia imagen de huella dactilar");
                            cS.enviaCadena("VERIFICA_HUELLA");
                            using (var ms = new MemoryStream())
                            {
                                //Image image = Image.FromFile(@"C:\Users\PC_STE_19\Documents\Visual Studio 2015\Projects\BiometricFinger\alterImages\020_2_2_muchas_lineas.jpg", true);
                                Image image = buffer.huella;
                                image.Save(ms, ImageFormat.Jpeg);
                                cS.enviaImagen(image);
                            }
                            estado = "RECIBE_USUARIO";
                            break;
                        case "RECIBE_USUARIO":
                            //bloquea hasta que un cliente envía imagen de dedo
                            Console.WriteLine("Recibe usuario correspondiente a la huella dactilar");
                            //Usuario usuario = cS.recibeUsuario();
                            string sUsuario = cS.leeCadena();

                            if (sUsuario == "NO VERIFICADO")
                            {
                                estadoHuella = -1;
                                estado = "FIN";
                            }
                            else
                            {
                                estadoHuella = 1;
                                estado = "ENVIA_OPERACION";
                            }
                            Console.WriteLine(sUsuario);
                            break;
                        case "ENVIA_OPERACION":
                            cS.enviaCadena("RECIBE_OPERACION");
                            //bloquea hasta que un cliente envía imagen de dedo
                            Console.WriteLine("Recibe usuario correspondiente a la huella dactilar");
                            cS.enviaCadena("ENTRADA");
                            estado = "CONFIRMACION";
                            break;
                        case "CONFIRMACION":
                            //bloquea hasta que un cliente envía imagen de dedo
                            Console.WriteLine("Recibe usuario correspondiente a la huella dactilar");
                            string confirmacion = cS.leeCadena();
                            Console.WriteLine(confirmacion);
                            estado = "FIN";
                            break;
                        case "FIN":
                            cS.enviaCadena("FIN");
                            started = false;
                            break;
                    }
                }
                catch
                {
                    //Se ha producido un error de socket
                    Console.WriteLine("Un error de socket ha ocurrido con el cliente: " + tcpClient.ToString());
                    break;
                }

                /*if (bytesRead == 0)
                {
                    //The server has disconnected
                    break;
                }*/

                Thread.Sleep(15);
            }

            started = false;
            Disconnect();
        }

        /// <summary>
        /// Adds data to the packet to be sent out, but does not send it across the network
        /// </summary>
        /// <param name="data">The data to be sent</param>
        public void AddToPacket(byte[] data)
        {
            if (buffer.CurrentWriteByteCount + data.Length > buffer.WriteBuffer.Length)
            {
                FlushData();
            }

            Array.ConstrainedCopy(data, 0, buffer.WriteBuffer, buffer.CurrentWriteByteCount, data.Length);
            buffer.CurrentWriteByteCount += data.Length;
        }

        /// <summary>
        /// Flushes all outgoing data to the server
        /// </summary>
        public void FlushData()
        {
            clientStream.Write(buffer.WriteBuffer, 0, buffer.CurrentWriteByteCount);
            clientStream.Flush();
            buffer.CurrentWriteByteCount = 0;
        }

        /// <summary>
        /// Sends the byte array data immediately to the server
        /// </summary>
        /// <param name="data"></param>
        public void SendImmediate(byte[] data)
        {
            /*
            AddToPacket(data);
            FlushData();*/

            clientStream.WriteByte((byte)(data.Length / 256));
            clientStream.WriteByte((byte)(data.Length & 255));
            clientStream.Write(data, 0, data.Length);
            clientStream.Flush();
            buffer.CurrentWriteByteCount = 0;
        }

        /// <summary>
        /// Tells whether we're connected to the server
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return started && tcpClient.Connected;
        }

        /// <summary>
        /// Disconnect from the server
        /// </summary>
        public void Disconnect()
        {
            if (tcpClient == null)
            {
                return;
            }

            Console.WriteLine("Disconnected from server");

            tcpClient.Close();

            started = false;
        }

        public void setHuella(Image huella)
        {
            buffer.huella = huella;
        }
    }
}
