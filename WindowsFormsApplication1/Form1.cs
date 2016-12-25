using Membr_Potench.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using Microsoft.SqlServer.Server;
using System.Windows.Forms.DataVisualization.Charting;

namespace Membr_Potench
{
    public partial class Form1 : Form
    {
        int[] pointNumber = new int[6];
  
        public Form1()
        {
            InitializeComponent();
      
        }

        void Timers(object sender, EventArgs e)
        {
            button1_Click_1(sender, e);
        }

        void results(double time1, double[,] U, int n, int m, int Sh, int scale, int k, double Umax)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            for (int i = 0; i < n; ++i)
            {
                var stolb = new DataGridViewColumn();
                stolb.HeaderText = " " + i;
                stolb.Width = Sh;
                stolb.ReadOnly = true;
                stolb.Name = "i" + i;
                stolb.CellTemplate = new DataGridViewTextBoxCell();
                dataGridView1.Columns.Add(stolb);
            }

            dataGridView1.AllowUserToAddRows = false;

            for (int i = 0; i < n; ++i)
            {
                dataGridView1.Rows.Add();
                for (int j = 0; j < m; ++j)
                {
                    dataGridView1["i" + j, dataGridView1.Rows.Count - 1].Value = "[" + i + "," + j + "]" + "="/* + U[i, j]*/+ Math.Round(U[i, j], 2);

                }
            }
            if (chbParallel.Checked)
            {
                richTextBox1.AppendText("   Параллел. время: " + time1.ToString());
                //label12.Text = time1.ToString();
            }
            else
            {
                richTextBox1.AppendText("   Послед. время: " + time1.ToString());
            }

            if (checkBox1.Checked && checkBox3.Checked)
            {
                graphDraw(chart2, U, n, m, Umax);
            }
            if (checkBox5.Checked && checkBox3.Checked)
            {
                graphDraw(chart1, U, n, m, Umax);
            }

            if (checkBox5.Checked && checkBox4.Checked)
            {
                graphDraw(chart3, U, n, m, Umax);
            }
            if (checkBox1.Checked && checkBox4.Checked)
            {
                graphDraw(chart4, U, n, m, Umax);
            }

        }

        void graphDraw(Chart chart, double[,] U, int n, int m, double Umax)
        {
            chart.Series[0].Points.Clear();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (U[i, j] <= Umax * 0.15 && U[i, j] >= 0)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.White });
                    }
                    else if (U[i, j] > Umax * 0.15 && U[i, j] <= 0.3 * Umax)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.WhiteSmoke });
                    }
                    else if (U[i, j] > Umax * 0.3 && U[i, j] <= 0.5 * Umax)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.Blue });
                    }
                    else if (U[i, j] > 0.5 * Umax && U[i, j] <= 0.7 * Umax)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.Yellow });
                    }
                    else if (U[i, j] > 0.7 * Umax && U[i, j] <= 0.85 * Umax)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.OrangeRed });
                    }


                    else if (U[i, j] < 0 && U[i, j] >= -Umax * 0.15)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.WhiteSmoke });
                    }

                    else if (U[i, j] < -Umax * 0.15 && U[i, j] >= 0.3 * -Umax)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.YellowGreen });
                    }
                   
                    else if (U[i, j] < -Umax * 0.3 && U[i, j] >= 0.5 * -Umax)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.CadetBlue });
                    }
                    else if (U[i, j] < 0.5 * -Umax && U[i, j] >= 0.7 * -Umax)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.DarkSlateBlue });
                    }
                    else if (U[i, j] < 0.7 * -Umax && U[i, j] >= 0.85 * -Umax)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.DarkBlue });
                    }
                    else if (U[i, j] < 0.85 * -Umax)
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.Black });
                    }

                    else
                    {
                        chart.Series[0].Points.Add(new DataPoint(i, j) { Color = Color.Red });
                    }
                    //chart.Series[0].Points[j].ToolTip = string.Format("({0}, {1}) {2}", j, i, Math.Round(U[j, i], 2));

                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //richTextBox1.Clear();
            Timer timer1 = new Timer();
            timer1.Interval = 5000;
            timer1.Tick += Timers;
            //timer1.Start();

            double c = Convert.ToDouble(textBox7.Text);
            double a = Convert.ToDouble(textBox1.Text);
            double b = Convert.ToDouble(textBox2.Text);
            double h = Convert.ToDouble(textBox3.Text);
            double Umax = Convert.ToDouble(textBox11.Text);
            if (checkBox4.Checked)
            {
                Umax = Convert.ToDouble(textBox11.Text) / 5;
            }
            int n = (int)(a / h);
            int m = (int)(b / h);
            int l = Convert.ToInt32(textBox4.Text);
            double time2 = 0;
            double time3 = 0;
            double tau = 0.5;
            int x = (int)(n / 20);
            int y = (int)(m / 20);
            double gX = Convert.ToDouble(textBox5.Text);
            double gY = Convert.ToDouble(textBox6.Text);
            int Sh = Convert.ToInt32(textBox9.Text);
            int scale = Convert.ToInt32(textBox10.Text);
            int k = 0;
            double R = c * c * tau * tau / (h * h);
            Membr membr = new Membr(c, a, b, h, n, m, tau, l, gX, gY, scale);
            double[,] U = new double[n, m];
            //membr.Posledov(out time2);

            if (chbParallel.Checked)
            {
                if (checkBox3.Checked)
                    U = membr.Posledov(out time3);
                    membr.Parallel(out time2);
                if (checkBox4.Checked)
                    U = membr.ParallNE(out time2, R);
            }
            else
            {

                if (checkBox3.Checked)
                    U = membr.Posledov(out time2);
                if (checkBox4.Checked)
                    U = membr.PosledNE(out time2, R);

            }

            results(time2, U, n, m, Sh, scale, k, Umax);
            textBox4.Text = Convert.ToString(l + 1);

        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void chbParallel_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            double d = Convert.ToDouble(textBox8.Text);
            double a = Convert.ToDouble(textBox1.Text);
            double b = Convert.ToDouble(textBox2.Text);
            double h = Convert.ToDouble(textBox3.Text);
            double Umax = Convert.ToDouble(textBox11.Text);

            if (checkBox4.Checked)
            {
                Umax = Convert.ToDouble(textBox11.Text) / 10;
            }
            int n = (int)(a / h);
            int m = (int)(b / h);
            int l = Convert.ToInt32(textBox4.Text);
            double time1 = 0;
            double time3 = 0;
            double tau = 0.5;
            double gX = Convert.ToDouble(textBox5.Text);
            double gY = Convert.ToDouble(textBox6.Text);

            int Sh = Convert.ToInt32(textBox9.Text);
            int scale = Convert.ToInt32(textBox10.Text);
            int k = 0;
            double R = d * d * tau * tau / (h * h);
            Potench pot = new Potench(d, tau, a, b, h, n, m, l, gX, gY, scale);

            double[,] U = new double[n, m];

            if (chbParallel.Checked)
            {
                if (checkBox3.Checked)
                    U = pot.Posledov(out time3);
                    pot.Parallel(out time1);
                if (checkBox4.Checked)
                    U = pot.ParallNE(out time1, R);
            }
            else
            {

                if (checkBox3.Checked)
                    U = pot.Posledov(out time1);

                if (checkBox4.Checked)
                    U = pot.PosledNE(out time1, R);

            }

            results(time1, U, n, m, Sh, scale, k, Umax);
            textBox4.Text = Convert.ToString(l + 1);
        }
    }
}
