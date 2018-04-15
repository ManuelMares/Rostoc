
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * Éste contendrá los datos de cada fuerza en la viga, incluyendo, pero no limitándose, a apoyos.
 */
public class FuerzaYDistancia {

    /**
     * Éste contendrá los datos de cada fuerza en la viga, incluyendo, pero no limitándose, a apoyos.
     * @param distancia 
     * @param magnitud
     */
    public FuerzaYDistancia(double distancia, double magnitud)
    {
        this.distancia = distancia;
        this.magnitud = magnitud;
    }

    /**
     * 
     */
    public double distancia { get; set; }

    /**
     * 
     */
    public double magnitud { get; set; }


}