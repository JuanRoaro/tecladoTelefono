using System;
using System.Windows;
using Microsoft.Win32;
using NAudio.Wave;
using System.Linq;

namespace GraficadorSeñales
{
    public partial class MainWindow : Window
    {
        double amplitudMaxima = 1;
        Señal señal;
        Señal señalResultado;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnGraficar_Click(object sender, RoutedEventArgs e)
        {

            var reader = new AudioFileReader(txtRutaArchivo.Text);

            double tiempoInicial = 0;
            double tiempoFinal = reader.TotalTime.TotalSeconds;
            double frecuenciaMuestreo = reader.WaveFormat.SampleRate;

            txtFrecuenciaMuestreo.Text = frecuenciaMuestreo.ToString();
            txtTiempoInicial.Text = "0";
            txtTiempoFinal.Text = tiempoFinal.ToString();

            señal = new SeñalPersonalizada();

            //---------------------------------PRIMERA SEÑAL------------------------------------------------------//
            señal.TiempoInicial = tiempoInicial;
            señal.TiempoFinal = tiempoFinal;
            señal.FrecuenciaMuestreo = frecuenciaMuestreo;

            //Construir nuestra señal a traves del archivo de audio
            var bufferLectura = new float[reader.WaveFormat.Channels];
            int muestrasLeidas = 1;
            double instanteActual = 0;
            double intervaloMuestra = 1.0 / frecuenciaMuestreo;

            do
            {
                muestrasLeidas = reader.Read(bufferLectura, 0, reader.WaveFormat.Channels);
                if (muestrasLeidas > 0)
                {
                    double max = bufferLectura.Take(muestrasLeidas).Max();
                    señal.Muestras.Add(new Muestra(instanteActual, max));
                }
                instanteActual += intervaloMuestra;
            } while (muestrasLeidas > 0);


            señal.actualizarAmplitudMaxima();

            amplitudMaxima = señal.AmplitudMaxima;

            plnGrafica.Points.Clear();

            lblAmplitudMaximaY.Text = amplitudMaxima.ToString("F");
            lblAmplitudMaximaNegativaY.Text = "-" + amplitudMaxima.ToString("F");

            //PRIMERA SEÑAL
            if (señal != null)
            {
                //Recorre todos los elementos de una coleccion o arreglo
                foreach (Muestra muestra in señal.Muestras)
                {
                    plnGrafica.Points.Add(new Point((muestra.X - tiempoInicial) * scrContenedor.Width, (muestra.Y /
                        amplitudMaxima * ((scrContenedor.Height / 2.0) - 30) * -1) +
                        (scrContenedor.Height / 2)));

                }

            }


            plnEjeX.Points.Clear();
            //Punto del principio
            plnEjeX.Points.Add(new Point(0, (scrContenedor.Height / 2)));
            //Punto del final
            plnEjeX.Points.Add(new Point((tiempoFinal - tiempoInicial) * scrContenedor.Width,
                (scrContenedor.Height / 2)));

            plnEjeY.Points.Clear();
            //Punto del principio
            plnEjeY.Points.Add(new Point((0 - tiempoInicial) * scrContenedor.Width, (señal.AmplitudMaxima *
                ((scrContenedor.Height / 2.0) - 30) * -1) + (scrContenedor.Height / 2)));
            //Punto del final
            plnEjeY.Points.Add(new Point((0 - tiempoInicial) * scrContenedor.Width, (-señal.AmplitudMaxima *
                ((scrContenedor.Height / 2.0) - 30) * -1) + (scrContenedor.Height / 2)));
        }

        private void btnTransformadaFourier_Click(object sender, RoutedEventArgs e)
        {
            Señal transformada = Señal.transformar(señal);
            transformada.actualizarAmplitudMaxima();

            plnGraficaResultado.Points.Clear();

            lblAmplitudMaximaY_Resultado.Text = transformada.AmplitudMaxima.ToString("F");
            lblAmplitudMaximaNegativaY_Resultado.Text = "-" + transformada.AmplitudMaxima.ToString("F");

            //PRIMERA SEÑAL
            if (transformada != null)
            {
                //Recorre todos los elementos de una coleccion o arreglo
                foreach (Muestra muestra in transformada.Muestras)
                {
                    plnGraficaResultado.Points.Add(new Point((muestra.X - transformada.TiempoInicial) * scrContenedor_Resultado.Width, (muestra.Y /
                        transformada.AmplitudMaxima * ((scrContenedor_Resultado.Height / 2.0) - 30) * -1) +
                        (scrContenedor_Resultado.Height / 2)));
                }

                int indiceMinimoFrecuenciasBajas = 0;
                int indiceMaximoFrecuenciasBajas = 0;
                int indiceMinimoFrecuenciasAltas = 0;
                int indiceMaximoFrecuenciasAltas = 0;

                indiceMinimoFrecuenciasBajas = 680 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;
                indiceMaximoFrecuenciasBajas = 1000 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;
                indiceMinimoFrecuenciasAltas = 1200 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;
                indiceMaximoFrecuenciasAltas = 1500 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;

                double valorMaximoBajo = 0;
                int indiceMaximoBajo = 0;

                for (int indiceActual = indiceMinimoFrecuenciasBajas; indiceActual < indiceMaximoFrecuenciasBajas; indiceActual++)
                {
                    if (transformada.Muestras[indiceActual].Y > valorMaximoBajo)
                    {
                        valorMaximoBajo = transformada.Muestras[indiceActual].Y;
                        indiceMaximoBajo = indiceActual;
                    }
                }

                double valorMaximoAlto = 0;
                int indiceMaximoAlto = 0;

                for (int indiceActual = indiceMinimoFrecuenciasAltas; indiceActual < indiceMaximoFrecuenciasAltas; indiceActual++)
                {
                    if (transformada.Muestras[indiceActual].Y > valorMaximoAlto)
                    {
                        valorMaximoAlto = transformada.Muestras[indiceActual].Y;
                        indiceMaximoAlto = indiceActual;
                    }
                }

                double frecuenciaFundamentalBaja = (double)indiceMaximoBajo * señal.FrecuenciaMuestreo / (double)transformada.Muestras.Count;

                double frecuenciaFundamentalAlta = (double)indiceMaximoAlto * señal.FrecuenciaMuestreo / (double)transformada.Muestras.Count;


                if (frecuenciaFundamentalBaja > 694 && frecuenciaFundamentalBaja < 700)
                {
                    if (frecuenciaFundamentalAlta > 1206 && frecuenciaFundamentalAlta < 1215)
                    {
                        Hertz.Text = "1";
                    }
                    else
                    if (frecuenciaFundamentalAlta > 1330 && frecuenciaFundamentalAlta < 1340)
                    {
                        Hertz.Text = "2";
                    }
                    else
                    if (frecuenciaFundamentalAlta > 1470 && frecuenciaFundamentalAlta < 1480)
                    {
                        Hertz.Text = "3";
                    }
                }
                else
                if (frecuenciaFundamentalBaja > 765 && frecuenciaFundamentalBaja < 780)
                {
                    if (frecuenciaFundamentalAlta > 1206 && frecuenciaFundamentalAlta < 1215)
                    {
                        Hertz.Text = "4";
                    }
                    else
                    if (frecuenciaFundamentalAlta > 1330 && frecuenciaFundamentalAlta < 1340)
                    {
                        Hertz.Text = "5";
                    }
                    else
                    if (frecuenciaFundamentalAlta > 1470 && frecuenciaFundamentalAlta < 1480)
                    {
                        Hertz.Text = "6";
                    }
                }
                else
                if (frecuenciaFundamentalBaja > 845 && frecuenciaFundamentalBaja < 855)
                {
                    if (frecuenciaFundamentalAlta > 1206 && frecuenciaFundamentalAlta < 1215)
                    {
                        Hertz.Text = "7";
                    }
                    else
                    if (frecuenciaFundamentalAlta > 1330 && frecuenciaFundamentalAlta < 1340)
                    {
                        Hertz.Text = "8";
                    }
                    else
                    if (frecuenciaFundamentalAlta > 1470 && frecuenciaFundamentalAlta < 1480)
                    {
                        Hertz.Text = "9";
                    }
                }
                else
                if (frecuenciaFundamentalBaja > 935 && frecuenciaFundamentalBaja < 945)
                {
                    if (frecuenciaFundamentalAlta > 1206 && frecuenciaFundamentalAlta < 1215)
                    {
                        Hertz.Text = "*";
                    }
                    else
                    if (frecuenciaFundamentalAlta > 1330 && frecuenciaFundamentalAlta < 1340)
                    {
                        Hertz.Text = "0";
                    }
                    else
                    if (frecuenciaFundamentalAlta > 1470 && frecuenciaFundamentalAlta < 1480)
                    {
                        Hertz.Text = "#";
                    }
                }


            }



            plnEjeXResultado.Points.Clear();
            //Punto del principio
            plnEjeXResultado.Points.Add(new Point(0, (scrContenedor_Resultado.Height / 2)));
            //Punto del final
            plnEjeXResultado.Points.Add(new Point((transformada.TiempoFinal - transformada.TiempoInicial) * scrContenedor_Resultado.Width,
                (scrContenedor_Resultado.Height / 2)));

            plnEjeYResultado.Points.Clear();
            //Punto del principio
            plnEjeYResultado.Points.Add(new Point((0 - transformada.TiempoInicial) * scrContenedor_Resultado.Width, (transformada.AmplitudMaxima *
                ((scrContenedor_Resultado.Height / 2.0) - 30) * -1) + (scrContenedor_Resultado.Height / 2)));
            //Punto del final
            plnEjeYResultado.Points.Add(new Point((0 - transformada.TiempoInicial) * scrContenedor_Resultado.Width, (-transformada.AmplitudMaxima *
                ((scrContenedor_Resultado.Height / 2.0) - 30) * -1) + (scrContenedor_Resultado.Height / 2)));
        }

        private void btnExaminar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            if ((bool)fileDialog.ShowDialog())
            {
                txtRutaArchivo.Text = fileDialog.FileName;
            }
        }
    }

}
