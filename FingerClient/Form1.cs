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
        Client client = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void botonMostrar_Click(object sender, EventArgs e)
        {
            // Muestra el cuadro de diálogo Abrir archivo. Si el usuario hace clic en OK, cargua la 
            // imagen que el usuario elije.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Load(openFileDialog1.FileName);
                huella = pictureBox1.Image;
            }
        }

        private void botonBorrar_Click(object sender, EventArgs e)
        {
            //Elimina la imagen
            pictureBox1.Image = null;
        }

        private void comparaHuella_Click(object sender, EventArgs e)
        {
            //Muestra el color del dialogo. Si el usuario hace click en OK,
            //cambia PictureBox al color que el suario a seleccionado.
            /*if (colorDialog1.ShowDialog() == DialogResult.OK)
                pictureBox1.BackColor = colorDialog1.Color;*/

            client = new Client();
            client.setHuella(huella);
            client.ConnectToServer("161.33.129.193", 8888);

            if (!client.IsConnected())
            {
                //ERROR CONEXION SERVIDOR
                pictureBox1.BackColor = Color.Red;
            }else
            {
                int count = 0;
                while (client.estadoHuella == 0)
                {
                    count++;
                }

                if(client.estadoHuella == 1)
                {
                    //IDENTIFICACION
                    pictureBox1.BackColor = Color.Green;
                }
                if(client.estadoHuella == -1)
                {
                    //ERROR IDENTIFICACION
                    pictureBox1.BackColor = Color.Yellow;
                }
            }
        }

        private void botonCerrar_Click(object sender, EventArgs e)
        {
            //Cierra el formulario
            client.Disconnect();
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            else
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
