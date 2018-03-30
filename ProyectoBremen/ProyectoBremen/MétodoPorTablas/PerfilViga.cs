
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * 
 */
public class PerfilViga {

    /**
     * 
     */
    public PerfilViga(int codigo)
    {
        this.codigo = codigo;
        this.momentoDeInercia = 1;
    }

    /**
     * 
     */
    public double centroide { get; set; }

    /**
     * 
     */
    public double areas { get; set; }

    /**
     * 
     */
    public List<double> distancias { get; set; }

    /**
     * 
     */
    public double centroLocal { get; set; }

    /**
     * Éste es el segundo momento polar de inercia
     */
    public double momentoDeInercia { get; set; }

    /**
     * 
     */
    public int codigo { get; set; }

    /**
     * 
     */
    public double valor { get; set; }


}