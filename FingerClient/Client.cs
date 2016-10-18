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
    /// Implementación de un cliente TCP el cual se conecta a un servidor específico 
    /// y plantea acontecimientos cuando se reciben datos desde el servidor
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
        public string sUsuario { get; set; }

        /// <summary>
        /// Construye un nuevo cliente
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
        /// Inicializa un cliente TCP a un servidor TCP con los parámetros de IP y Puerto recibidos
        /// </summary>
        /// <param name="ipAddress">Dirección IP (IPV4) del servidor</param>
        /// <param name="port">Puerto de escucha del servidor</param>
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
        /// Este método se ejecuta en su propio hilo, y es responsable de recibir 
        /// datos desde el servidor.
        /// </summary>
        private void ListenForPackets()
        {
            int bytesRead;
            string estado = "INICIAL";
            ComunicacionStream cS = new ComunicacionStream(clientStream);
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
                            Image image = buffer.huella;
                            image.Save("c:\\imagenCLIENTE.jpg");
                            cS.enviaImagen(image);
                            estado = "RECIBE_USUARIO";
                            break;
                        case "RECIBE_USUARIO":
                            //bloquea hasta que un cliente envía imagen de dedo
                            Console.WriteLine("Recibe usuario correspondiente a la huella dactilar");
                            //Usuario usuario = cS.recibeUsuario();
                            sUsuario = cS.leeCadena();

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
                catch (Exception e)
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
        /// Añade datos al paquete para ser enviado, pero no es enviado a la red
        /// </summary>
        /// <param name="data">Datos para ser enviados</param>
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
        /// Vacía y envía todos los datos salientes.
        /// </summary>
        public void FlushData()
        {
            clientStream.Write(buffer.WriteBuffer, 0, buffer.CurrentWriteByteCount);
            clientStream.Flush();
            buffer.CurrentWriteByteCount = 0;
        }

        /// <summary>
        /// Envia un array de bytes inmediatamente al servidor
        /// </summary>
        /// <param name="data">Datos para enviar</param>
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
        /// Indica si estamos conectados
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return started && tcpClient.Connected;
        }

        /// <summary>
        /// Desconexión del servidor
        /// </summary>
        public void Disconnect()
        {
            if (tcpClient == null)
            {
                return;
            }

            Console.WriteLine("Desconexión del servidor");

            tcpClient.Close();

            started = false;
        }
        /// <summary>
        /// Recibe una imagen de huella dactilar
        /// </summary>
        /// <param name="data">Imagen de huella</param>
        public void setHuella(Image huella)
        {
            buffer.huella = huella;
        }
    }
}
