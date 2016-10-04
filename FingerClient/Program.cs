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
        private static int numClients = 1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Thread[] server = new Thread[numClients];
            for (int i = 0; i < numClients; i++)
            {
                server[i] = new Thread(conexion);
                server[i].Start();
            }
            Thread.Sleep(250);
            /*if (Args.Length > 0)
            {
                if (Args[0] == "spawnclient")
                {*/

            /*    }
            }*/
            /*else
            {
                Console.WriteLine("\n*** Named pipe client stream with impersonation example ***\n");
                StartClients();
            }*/

            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/
        }

        private static void conexion()
        {
            NamedPipeClientStream pipeClient =
                        new NamedPipeClientStream(".", "testfinger",
                            PipeDirection.InOut, PipeOptions.None,
                            TokenImpersonationLevel.Impersonation);

            Console.WriteLine("\nConnecting to server...");
            pipeClient.Connect();

            StreamString ss = new StreamString(pipeClient);
            // Validate the server's signature string
            if (ss.ReadString() == "I am the one true server!")
            {
                // The client security token is sent with the first write.
                // Send the name of the file whose contents are returned
                // by the server.
                ss.WriteString("c:\\textfile.txt");

                // Print the file to the screen.
                Console.Write(ss.ReadString());
            }
            else
            {
                Console.WriteLine("Server could not be verified. \n");
            }
            pipeClient.Close();
            // Give the client process some time to display results before exiting.
            Thread.Sleep(4000);
        }
    }

    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            Image image = Image.FromFile(@"C:\Users\PC_STE_19\Documents\Visual Studio 2015\Projects\BiometricFinger\alterImages\020_2_2_muchas_lineas.jpg", true);
            byte[] imageData;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                imageData = ms.ToArray();
            }

            //byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = imageData.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }

            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(imageData, 0, len);
            ioStream.Flush();

            return 1;
        }
    }
}