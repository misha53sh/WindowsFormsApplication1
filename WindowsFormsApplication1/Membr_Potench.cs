using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using Microsoft.SqlServer.Server;

namespace Membr_Potench.Classes
{
    abstract class Membr_Potench
    {
        protected int razmGraf;
        protected double xmax, ymax;
        protected double h;
        protected int zX, zY;
        protected double[,] Uk;
        protected int time;
        protected double grX, grY;
        protected double[,] Uminus;
        protected double[,] Uplus;
        protected int l;

        protected Membr_Potench(double A, double B, double H, int N, int M, int L, double gX, double gY, int scalee)
        {
            this.xmax = A;
            this.ymax = B;
            this.h = H;
            this.zX = N;
            this.zY = M;
            this.time = L;
            this.grX = gX;
            this.grY = gY;
            this.razmGraf = scalee;
            this.l = L;
            Uminus = new double[zX, zY];
            Uplus = new double[zX, zY];
            Uk = new double[zX, zY];
            for (var i = 1; i < zX - 1; i++)
            {
                for (var j = 1; j < zY - 1; j++)
                {
                    Uplus[i, j] = Uk[i, j];
                    Uminus[i, j] = Uk[i, j];
                }
            }
        }

        protected void GranichYsl()
        {
            for (int i = 0; i < zX; i++)
                Uk[i, zY - 1] = 0;
            for (int i = 0; i < zY; i++)
                Uk[zX - 1, i] = 0;
            for (int i = 0; i < zX; i++)
                //Uk[i, 0] = grX;
                Uk[i, 0] = 0;
            for (int i = 0; i < zY; i++)
                //Uk[0, i] = grY;
                Uk[0, i] = 0;
            for (var i = 1; i < zX - 1; i++)
            {
                for (var j = 1; j < zY - 1; j++)
                {
                    Uk[j, i] = 0.0001 * (i * h) * (j * h) * (zX * h - i * h) * (zX * h - j * h);
                }
            }
        }
        protected void GranichYslTep()
        {
            for (var i = 0; i < zX; i++)
            {
                for (var j = 0; j < zY; j++)
                {
                    Uk[j, i] = 0;
                    Uk[9, 9] = 100;
                }
            }
        }

        public double[,] Posledov(out double PoslTime)
        {
            Stopwatch sw = new Stopwatch();
            PoslTime = 0;
            sw.Start();
            PosledovCalc();
            sw.Stop();
            PoslTime = sw.ElapsedMilliseconds;
            return Uk;
        }

        public double[,] Parallel(out double ParallelTime)
        {
            Stopwatch sw = new Stopwatch();
            ParallelTime = 0;
            sw.Start();
            ParallelCalc();
            sw.Stop();
            ParallelTime = sw.ElapsedMilliseconds;
            return Uk;
        }

        protected void PosledovCalc()
        {
            while (time > 0)
            {
                for (int i = 1; i < zY - 1; i++)
                    for (int j = 1; j < zX - 1; j++)
                        yravnenie(j, i);
                for (int i = 1; i < zY - 1; i++)
                    for (int j = 1; j < zY - 1; j++)
                    {
                        Uminus[j, i] = Uk[j, i];
                        Uk[j, i] = Uplus[j, i];
                    }
                time--;
            }
        }

        protected void ParallelCalc()
        {
            Task[] tasks = new Task[4];
            for (int numberTask = 0; numberTask < 4; numberTask++)
            {
                int z = numberTask;
                tasks[numberTask] = Task.Run(() =>
                {
                    int timeTask = 0;
                    int calcInterval = z * (zX - 1) / 4;
                    while (timeTask < time)
                    {
                        for (int i = 1; i < zY - 1; i++)
                            for (int j = calcInterval + 1; j < calcInterval + (zX - 1) / 4; j++)
                                yravnenie(j, i);
                        for (int i = 1; i < zY - 1; i++)
                            for (int j = calcInterval + 1; j < calcInterval + (zX - 1) / 4; j++)
                            {
                                Uminus[j, i] = Uk[j, i];
                                Uk[j, i] = Uplus[j, i];
                            }
                        timeTask++;
                    }
                });
            }
            Task.WaitAll(tasks);
        }
        public double[,] PosledNE(out double poslTimer, double R)
        {
            int timer = l;
            Stopwatch sw = new Stopwatch();
            poslTimer = 0;
            sw.Start();
            PaschProg(R, timer);
            sw.Stop();
            poslTimer = sw.ElapsedMilliseconds*2;
            return Uk;
        }

        public double[,] ParallNE(out double parallTimer, double R)
        {
            int iteratParallel = l;
            Stopwatch sw = new Stopwatch();
            parallTimer = 0;
            sw.Start();
            PaschProg(R, iteratParallel);
            sw.Stop();
            parallTimer = sw.ElapsedMilliseconds;
            return Uk;
        }
        public void PaschProg(double R, int Timer)
        {
            double[,] L = new double[zX, zY];
            double[,] M = new double[zX, zY];

            for (int i = 1; i < zY - 1; i++)
            {
                L[0, i] = 0;
                M[0, i] = Uk[0, i];
            }
            for (int i = 1; i < zX - 1; i++)
            {
                L[i, 0] = 0;
                M[i, 0] = Uk[i, 0];
            }
            for (int i = 1; i < zY - 1; i++)
            {
                L[zX - 1, i] = 0;
                M[zX - 1, i] = Uk[zX - 1, i];
            }
            for (int i = 1; i < zX - 1; i++)
            {
                L[i, zY - 1] = 0;
                M[i, zY - 1] = Uk[i, zY - 1];
            }
            while (Timer > 0)
            {
                ProgLev(R, L, M);
                ProgSprav(R, L, M);
                Timer--;
            }
        }

        public void ProgLev(double R, double[,] L, double[,] M)
        {
            for (int i = 1; i < zY - 1; i++)
                for (int j = 1; j < zX - 1; j++)
                {
                    L[j, i] = R / (1 + 2 * R - R * L[j - 1, i]);
                    M[j, i] = (Uk[j - 1, i] + R * M[j - 1, i]) / (1 + 2 * R - R * L[j - 1, i]);
                }
            for (int i = 1; i < zY - 1; i++)
                for (int j = zX - 2; j > 0; j--)
                    Uk[j, i] = (L[j + 1, i] * Uk[j + 1, i] + M[j + 1, i]);
        }

        public void ProgSprav(double R, double[,] L, double[,] M)
        {
            for (int j = 1; j < zX - 1; j++)
                for (int i = 1; i < zY - 1; i++)
                {
                    L[j, i] = R / (1 + 2 * R - R * L[j, i - 1]);
                    M[j, i] = (Uk[j, i - 1] + R * M[j, i - 1]) / (1 + 2 * R - R * L[j, i - 1]);
                }
            for (int j = 1; j < zX - 1; j++)
                for (int i = zY - 2; i > 0; i--)
                    Uk[j, i] = (L[j, i + 1] * Uk[j, i + 1] + M[j, i + 1]);
        }

        protected abstract void yravnenie(int i, int j);
        protected abstract void yravnenieProg(int i, int j);
        protected abstract void yravnenieProgonki(int i, int j);
    }

    class Membr : Membr_Potench
    {
        double c;
        double tay;
        public Membr(double C, double A, double B, double H, int N, int M, double Tay, int L, double gX, double gY, int scalee)
            : base(A, B, H, N, M, L, gX, gY, scalee)
        {
            this.c = C;
            this.tay = Tay;
            GranichYslTep();
        }

        protected override void yravnenie(int j, int i)
        {
            Uplus[j, i] = 2.0 * Uk[j, i] - Uminus[j, i] + c * c * (tay * tay) / (h * h) * (Uk[j - 1, i] + Uk[j + 1, i] + Uk[j, i - 1] + Uk[j, i + 1] - 4 * Uk[j, i]);
        }

        protected override void yravnenieProg(int i, int j) { }
        protected override void yravnenieProgonki(int i, int j) { }
    }

    class Potench : Membr_Potench
    {
        double tay;
        double d;
        public Potench(double D, double Tay, double A, double B, double H, int N, int M, int L, double gX, double gY, int scalee)
            : base(A, B, H, N, M, L, gX, gY, scalee)
        {
            this.tay = Tay;
            this.d = D;
            GranichYslTep();
        }

        protected override void yravnenie(int j, int i)
        {
           double tau = 0.1;
            if (i == 9 && j == 9)
            { Uplus[j, i] = 100; }
            else
            {
                Uplus[j, i] = Uk[j, i] + ((d * d * tau) / (h * h)) * (Uk[j, i - 1] + Uk[j, i + 1] - 4 * Uk[j, i] + Uk[j - 1, i] + Uk[j + 1, i]);
            }
        }
        protected override void yravnenieProg(int i, int j) { }
        protected override void yravnenieProgonki(int i, int j) { }
    }
}


