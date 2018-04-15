using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoBremen.MétodoPorTablas
{
    public class CargaDistribuida
    {
        public CargaDistribuida(double di, double df, double m)
        {
            magnitudInicial = m;
            distanciaInicial = di;
            distanciaFinal = df;
        }
        public CargaDistribuida(double di, double df, double mi, double mf)
        {
            magnitudInicial = mi;
            magnitudFinal = mf;
            distanciaInicial = di;
            distanciaFinal = df;
        }

        public double magnitudInicial { get; set; }
        public double magnitudFinal { get; set; }
        public double distanciaInicial { get; set; }
        public double distanciaFinal { get; set; }
    }
}
