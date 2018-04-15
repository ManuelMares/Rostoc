using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoBremen.MétodoPorTablas
{
    public class Primitivos
    {
        public Primitivos(double longitud, double apoyo1, double apoyo2)
        {
            this.apoyo1 = apoyo1;
            this.apoyo2 = apoyo2;
            this.longitud = longitud;
        }

        /**
         * 
         */
        public double apoyo1 { get; set; }

        /**
         * 
         */
        public double apoyo2 { get; set; }

        /**
         * 
         */
        public double longitud { get; set; }
    }
}
