using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoBremen.MétodoPorTablas
{
    public class Momento
    {
        public Momento(double magnitud, double codigo)
        {
            this.magnitud = magnitud;
            this.distancia = codigo;
            // TODO implement here
        }

        public double magnitud { get; set; }
        public double distancia { get; set; }
    }
}
