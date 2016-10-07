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

            NamedPipeClientStream pipeCliente = new NamedPipeClientStream("161.33.129.189", "testfinger", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            pipeCliente.Connect();

            ComunicacionStream cS = new ComunicacionStream(pipeCliente);
            if(cS.leeCadena() == "Conectado al servidor"){
                cS.enviaImagen(huella);

                if(cS.leeCadena() == "IDENTIFICADO"){
                    string nombreUsuario = cS.leeCadena();
                    pictureBox1.BackColor = Color.Green;
                }
                else{
                    //ERROR IDENTIFICACION
                    pictureBox1.BackColor = Color.Yellow;
                }
            }else{
                //ERROR CONEXION SERVIDOR
                pictureBox1.BackColor = Color.Red;
            }
            pipeCliente.Close();
        }

        private void botonCerrar_Click(object sender, EventArgs e)
        {
            //Cierra el formulario
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
