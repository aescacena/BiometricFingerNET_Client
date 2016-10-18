using SecuGen.FDxSDKPro.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FingerClient
{
    public partial class Form1 : Form
    {
        public Image huella = null;
        Client cliente = null;

        private SGFingerPrintManager m_FPM;

        private bool m_LedOn = false;
        private Int32 m_ImageWidth;
        private Int32 m_ImageHeight;

        private Int32 iError;
        private SGFPMDeviceName nombreDispositivo;
        private Int32 dispositivoID;
        private Int32 tiempo;
        private  Byte[] fp_image;

        public Form1()
        {
            m_FPM = new SGFingerPrintManager();
            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        /// <summary>
        /// Realiza la inicialización básica del dispositivo, la 
        /// lectura de la huella dactilar y la muestra en la aplicación
        /// </summary>
        private void botonMostrar_Click(object sender, EventArgs e)
        {
            Int32 iError;
            SGFPMDeviceName device_name;
            Int32 device_id;
            Int32 elap_time;

            Byte[] fp_image;

            device_name = SGFPMDeviceName.DEV_FDU05;
            device_id = (Int32)(SGFPMPortAddr.USB_AUTO_DETECT);

            iError = m_FPM.Init(device_name);
            iError = m_FPM.OpenDevice(device_id);

            SGFPMDeviceInfoParam pInfo = new SGFPMDeviceInfoParam();
            iError = m_FPM.GetDeviceInfo(pInfo);
            m_ImageWidth = pInfo.ImageWidth;
            m_ImageHeight = pInfo.ImageHeight;

            elap_time = Environment.TickCount;
            fp_image = new Byte[m_ImageWidth * m_ImageHeight];

            if (iError == (Int32)SGFPMError.ERROR_NONE)
                Console.WriteLine("Inicialización correcta");
            else
                Console.WriteLine("OpenDevice()", iError);

            iError = m_FPM.GetImage(fp_image);

            if (iError == (Int32)SGFPMError.ERROR_NONE)
            { 
                elap_time = Environment.TickCount - elap_time;
                Console.WriteLine("Tiempo de captura: " + elap_time + " ms");
            }
            else
                Console.WriteLine("OpenDevice()", iError);

            int colorval;
            Bitmap bmp = new Bitmap(m_ImageWidth, m_ImageHeight);
            pictureBox1.Image = (Image)bmp;
            huella = pictureBox1.Image;

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    colorval = (int)fp_image[(j * m_ImageWidth) + i];
                    bmp.SetPixel(i, j, Color.FromArgb(colorval, colorval, colorval));
                }
            }

            // Muestra el cuadro de diálogo Abrir archivo. Si el usuario hace clic en OK, cargua la 
            // imagen que el usuario elije.
            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            // {
            //     pictureBox1.Load(openFileDialog1.FileName);
            //     huella = pictureBox1.Image;
            // }
            pictureBox1.Refresh();
        }

        private void botonBorrar_Click(object sender, EventArgs e)
        {
            //Elimina la imagen
            pictureBox1.Image = null;
        }

        /// <summary>
        /// Construye un nuevo cliente TCP que se conecta a la IP y puerto dado
        /// </summary>
        private void comparaHuella_Click(object sender, EventArgs e)
        {
            cliente = new Client();
            cliente.setHuella(huella);
            //cliente.ConnectToServer("161.33.129.182", 8888);
            cliente.ConnectToServer("192.168.1.137", 8888);

            if (!cliente.IsConnected())
            {
                //ERROR CONEXION SERVIDOR
                pictureBox1.BackColor = Color.Red;
            }else
            {
                int count = 0;
                while (cliente.estadoHuella == 0)
                {
                    count++;
                }

                if(cliente.estadoHuella == 1)
                {
                    //IDENTIFICACION
                    pictureBox1.BackColor = Color.Green;
                }
                if(cliente.estadoHuella == -1)
                {
                    //ERROR IDENTIFICACION
                    pictureBox1.BackColor = Color.Yellow;
                }
            }
        }

        /// <summary>
        /// Elimina un cliente determinado de nuestra lista de clientes
        /// </summary>
        private void botonCerrar_Click(object sender, EventArgs e)
        {
            //Cierra el formulario
            cliente.Disconnect();
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Guardar_Click(object sender, EventArgs e)
        {
            huella.Save("c:\\imagenSecugen.jpg");
        }

        /// <summary>
        /// Realiza la inicialización básica del dispositivo lector de huella y
        /// para detectar automáticamente la lectura de la huella dactilar.
        /// </summary>
        /// <param name="client"></param>
        private void Inicializa_Click(object sender, EventArgs e)
        {
            nombreDispositivo = SGFPMDeviceName.DEV_FDU05;
            dispositivoID = (Int32)(SGFPMPortAddr.USB_AUTO_DETECT);

            iError = m_FPM.Init(nombreDispositivo);
            iError = m_FPM.OpenDevice(dispositivoID);

            m_FPM.EnableAutoOnEvent(true, (int)this.Handle);


            SGFPMDeviceInfoParam pInfo = new SGFPMDeviceInfoParam();
            iError = m_FPM.GetDeviceInfo(pInfo);
            m_ImageWidth = pInfo.ImageWidth;
            m_ImageHeight = pInfo.ImageHeight;
            fp_image = new Byte[m_ImageWidth * m_ImageHeight];

            if (iError == (Int32)SGFPMError.ERROR_NONE)
                mensajeEmergente("Inicializado correctamente", "Inicializado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                mensajeEmergente("Error al inicializar lector de huella", "Error "+ iError, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Función para mandar mensaje en una ventana emergente.
        /// </summary>
        /// <param name="texto">Texto descriptivo a mostrar</param>
        /// <param name="titulo">Texto a mostrar en la parte superior de la ventana</param>
        /// <param name="tipoBoton">Botones a mostrar en la ventana</param>
        /// <param name="tipoIcono">Icono descriptivo a mostrat</param>
        void mensajeEmergente(string texto, string titulo, MessageBoxButtons tipoBoton, MessageBoxIcon tipoIcono) {
            MessageBox.Show(texto, titulo, tipoBoton, tipoIcono);
        }

        /// <summary>
        /// Función llamada cuando el lector de huella detecta una lectura de
        /// huella dactilar, esta función muestra en la aplicacion la imagen 
        /// de la misma.
        /// </summary>
        /// <param name="m">Mensaje de estado recibido del lector de huella</param>
        protected override void WndProc(ref Message m)
        {
            if(m.Msg == (int)SGFPMMessages.DEV_AUTOONEVENT)
            {
                if (m.WParam.ToInt32() == (Int32)SGFPMAutoOnEvent.FINGER_ON)
                {
                    Console.WriteLine("Lector de huella ON");
                    muestraHuella();
                }
                else if (m.WParam.ToInt32() == (Int32)SGFPMAutoOnEvent.FINGER_OFF)
                    Console.WriteLine("Lector de huella OFF");
            }
            
            base.WndProc(ref m);
        }

        /// <summary>
        /// Realiza la lectura de la huella dactilar y la muestra en la aplicación
        /// </summary>
        private void muestraHuella()
        {
            tiempo = Environment.TickCount;

            iError = m_FPM.GetImage(fp_image);

            if (iError == (Int32)SGFPMError.ERROR_NONE)
            {
                tiempo = Environment.TickCount - tiempo;
                Console.WriteLine("Tiempo de captura: " + tiempo + " ms");
            }
            else
                Console.WriteLine("OpenDevice()", iError);

            int colorval;
            Bitmap bmp = new Bitmap(m_ImageWidth, m_ImageHeight);
            pictureBox1.Image = (Image)bmp;
            huella = pictureBox1.Image;

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    colorval = (int)fp_image[(j * m_ImageWidth) + i];
                    bmp.SetPixel(i, j, Color.FromArgb(colorval, colorval, colorval));
                }
            }

            // Muestra el cuadro de diálogo Abrir archivo. Si el usuario hace clic en OK, cargua la 
            // imagen que el usuario elije.
            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            // {
            //     pictureBox1.Load(openFileDialog1.FileName);
            //     huella = pictureBox1.Image;
            // }
            pictureBox1.Refresh();
        }
    }
}
