using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FingerClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            /*Thread[] server = new Thread[numClients];
            for (int i = 0; i < numClients; i++)
            {
                server[i] = new Thread(conexion);
                server[i].Start();
            }
            Thread.Sleep(250);*/

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            //Client client = new Client();
            //client.OnDataReceived += new ClientHandlePacketData(client_OnDataReceived);
            //client.ConnectToServer("161.33.129.193", 8888);
            //client.ConnectToServer("192.168.1.137", 8888);

            ASCIIEncoding encoder = new ASCIIEncoding();

            //string s = "test";

            /*while (true)
            {
                string s = Console.ReadLine();

                if (s != null)
                {
                    //If the user types exit, then exit
                    if ("exit".Equals(s))
                    {
                        break;
                    }

                    //client.SendImmediate(Encoding.ASCII.GetBytes(s));

                    byte[] imageData;
                    using (var ms = new MemoryStream())
                    {
                        //Image image = Image.FromFile(@"C:\Users\aesca\OneDrive\documentos\visual studio 2015\Projects\BiometricFinger\alterImages\020_2_2_muchas_lineas.jpg", true);
                        Image image = Image.FromFile(@"C:\Users\PC_STE_19\Documents\Visual Studio 2015\Projects\BiometricFinger\alterImages\020_2_2_muchas_lineas.jpg", true);
                        image.Save(ms, ImageFormat.Jpeg);
                        imageData = ms.ToArray();
                    }

                    //byte[] outBuffer = streamEncoding.GetBytes(outString);
                    int len = imageData.Length;
                    if (len > UInt16.MaxValue)
                    {
                        len = (int)UInt16.MaxValue;
                    }
                    client.SendImmediate(imageData);
                    s = null;
                }
            }

            client.Disconnect();
            Environment.Exit(0);*/
        }

        static void client_OnDataReceived(byte[] data, int bytesRead)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            string message = encoder.GetString(data, 0, bytesRead);
            Console.WriteLine("Received a message: " + message);
        }

        private static void conexion()
        {
            NamedPipeClientStream pipeClient = new NamedPipeClientStream("161.33.129.189", "testfinger", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);

            Console.WriteLine("\nConectando con el servidor...");
            pipeClient.Connect();

            ComunicacionStream cS = new ComunicacionStream(pipeClient);
            // Validate the server's signature string
            if (cS.leeCadena() == "Conectado al servidor"){
                // El token de seguridad del cliente se envía con la primera escritura.
                //cS.enviaImagen(Image.FromFile(@"C:\Users\PC_STE_19\Documents\Visual Studio 2015\Projects\BiometricFinger\alterImages\020_2_2_muchas_lineas.jpg", true);)

                if(cS.leeCadena() == "IDENTIFICADO")
                {
                    string nombreUsuario = cS.leeCadena();
                    Console.WriteLine("Nombre usuario: "+nombreUsuario);
                }
                else{
                    Console.WriteLine("No ha podido ser identificado");
                }
            }
            else{
                Console.WriteLine("El servidor no pudo ser verificado. \n");
            }
            pipeClient.Close();
            // Dar al proceso cliente un tiempo para mostrar resultados antes de salir.
            Thread.Sleep(4000);
        }
    }
}