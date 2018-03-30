
using NCalc;
using ProyectoBremen.Excepciones;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/**
 * 
 */
public class CalculadorPorTablas
{

    /**
     * 
     */
    public CalculadorPorTablas(PerfilViga perfilViga, Modulo modulo, List<FuerzaYDistancia> fuerzasYDistancias, string tipoDiagrama)
    {
        //Iniciación y declaración de variables
        this.perfilViga = perfilViga;
        this.modulo = modulo;
        this.fuerzasYDistancias = fuerzasYDistancias;
        this.tipoDiagrama = tipoDiagrama;
        this.deflexiones = new List<double>();
        this.momentos = new List<double>();
        this.reacciones = new List<double>();

        LlamarMetodosAsync();

    }

    private async System.Threading.Tasks.Task LlamarMetodosAsync()
    {
        await CalcularDeflexionAsync(fuerzasYDistancias, perfilViga, modulo, tipoDiagrama);
        this.CalcularMomento(fuerzasYDistancias);
        this.CalcularReaccion(fuerzasYDistancias);
    }

    /**
     * 
     */
    private string rutaDelDiagrama;

    /**
     * 
     */
    private PerfilViga perfilViga;

    /**
     * 
     */
    private Modulo modulo;

    /**
     * 
     */
    private string tipoDiagrama;

    /**
     * 
     */
    private List<FuerzaYDistancia> fuerzasYDistancias;

    /**
     * 
     */
    private List<double> deflexiones;

    /**
     * 
     */
    private List<double> momentos;

    /**
     * 
     */
    private List<double> reacciones;

    /**
     * 
     */
    private List<string> formulas { get; set; }

    /**
     * 
     */
    private string formulaAux { get; set; }
    

#region //Para la búsqueda y lectura de archivos
    /*
     * Este método busca el archivo en la carpeta de Assets, de no encontrarlo lanza una excepción de tipo 'FileNotFoundException'
     * @param nombreArchivoParaBuscar Es el nombre del archivo a buscar
     * @return Windows.Storage.StorageFile Regresa una variable que contiene el archivo listo para ser leído, e de tipo
     */
    private async System.Threading.Tasks.Task<Windows.Storage.StorageFile> BuscarArchivoAsync(string nombreArchivoParaBuscar)
    {
        Windows.Storage.StorageFile formulario;
        try
        {
            formulario = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\" + nombreArchivoParaBuscar);
        }
        catch (FileNotFoundException)
        {
            Windows.Storage.StorageFile bitacora = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\bitacoraErrores.txt");
            await Windows.Storage.FileIO.AppendTextAsync(bitacora, "\nNo se halló el directorio: " + nombreArchivoParaBuscar + ", fecha: " + new DateTime());
            throw new FileNotFoundException("No se halló la fórmula necesaría");
        }
        return formulario;
    }

    /*
     * Este método busca lee el archivo, de existir un problema con el archivo a buscar, lanza una excepción de tipo 'FileNotFoundException'
     * Separa el archivo con el delimitador '|'
     * @param nombreArchivoParaBuscar Es el nombre del archivo a buscar
     * @return string[] Regresa un arreglo de cadenas
     */
    private async System.Threading.Tasks.Task<List<string[]>> LeerArchivoAsync(Windows.Storage.StorageFile archivo)
    {
        List<string[]> formulario = new List<string[]>();
        try
        {
            List<string> renglonesLeidos = new List<string>(await Windows.Storage.FileIO.ReadLinesAsync(archivo));
            foreach (string renglon in renglonesLeidos)
            {
                formulario.Add(renglon.Split('|'));
            }
        }
        catch (FileNotFoundException)
        {
            Windows.Storage.StorageFile bitacora = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\bitacoraErrores.txt");
            await Windows.Storage.FileIO.AppendTextAsync(bitacora, "\nNo se halló el archivo: " + archivo + ", fecha: " + new DateTime());
            throw new FileNotFoundException("No se halló la fórmula necesaría");
        }
        return formulario;
    }
    #endregion

#region //Para la búsqueda de formula y guardado de la ruta del diagrama
    private async System.Threading.Tasks.Task<string[]> BuscarRenglonDeFormulasAsync(List<string[]> formulario, string tipoDiagrama)
    {
        try
        {
            foreach (string[] renglon in formulario)
            {
                if (renglon[0].Equals(tipoDiagrama.ToString()))
                {
                    return renglon;
                }
            }
            throw new FormulaNotFoundInFormulario();
        }
        catch (FormulaNotFoundInFormulario)
        {
            Windows.Storage.StorageFile bitacora = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\bitacoraErrores.txt");
            await Windows.Storage.FileIO.AppendTextAsync(bitacora, "\nNo se halló la fórmula: " + tipoDiagrama + "en el formulario, fecha: " + new DateTime());
            throw new FormulaNotFoundInFormulario("La fórmula con la etiqueta " + tipoDiagrama + ", no existe en el formulario"); ;
        }
    }

    private void GuardarRutaDiagrama(string ruta)
    {
        rutaDelDiagrama = ruta;
    }
    #endregion

#region //Para convertir el CSV en las fórmulas
    private void ConvertirFormularioAFormulas(string[] contenido)
    {
        string[] cadenasDeFormulas = contenido.Skip(2).ToArray();
        formulas = new List<string>();
        for (int i = 0; i < cadenasDeFormulas.Length; i++)
        {
            this.formulaAux = cadenasDeFormulas[i];
            ConvertirCadenaAFormula();
            this.formulas.Add(formulaAux);
        }
        this.formulaAux = null;
    }

    private void GuardarListaDeFormulas(List<string> formulas)
    {
        this.formulas = formulas;
    }

    private void ConvertirCadenaAFormula()
    {
        int estado = 1;
        while (estado != 0)
        {
            estado = ReemplazarFuerzaYDistancia();
        }
        estado = 1;
        while (estado != 0)
        {
            estado = ReemplazarModulo();
        }
        estado = 1;
        while (estado != 0)
        {
            estado = ReemplazarPerfilViga();
        }
    }

#region //reemplazador de valores
    private int ReemplazarFuerzaYDistancia()
    {
        string subCadena;
        try
        {
            subCadena = this.formulaAux.Substring(this.formulaAux.IndexOf("f"), 3);
        }
        catch (ArgumentOutOfRangeException)
        {
            return 0;
        }
        string valor;
        if (subCadena.ElementAt(1).ToString() == "d")
        {
            valor = this.fuerzasYDistancias[int.Parse(subCadena.Last().ToString())].distancia.ToString();
        }
        else if(subCadena.ElementAt(1).ToString() == "m")
        {
            valor = this.fuerzasYDistancias[int.Parse(subCadena.Last().ToString())].Magnitud.ToString();
        }
        else
        {
            throw new VarNotFoundInFormula();
        }
        this.formulaAux = this.formulaAux.Replace(subCadena, valor);
        return 1;
    }

    private int ReemplazarModulo()
    {
        string subCadena;
        try
        {
            subCadena = this.formulaAux.Substring(this.formulaAux.IndexOf("y"), 2);
            if (subCadena.Contains("a"))
                return 0;
        }
        catch (ArgumentOutOfRangeException)
        {
            return 0;
        }
        string valor;
        if (subCadena.ElementAt(1).ToString() == "k")
        {
            valor = this.modulo.codigo.ToString();
        }
        else if (subCadena.ElementAt(1).ToString() == "e")
        {
            valor = this.modulo.valor.ToString();

        }
        else
        {
            throw new VarNotFoundInFormula();
        }
        this.formulaAux = this.formulaAux.Replace(subCadena, valor);
        return 1;
    }

    private int ReemplazarPerfilViga()
    {
        string subCadena;
        try
        {
            subCadena = this.formulaAux.Substring(this.formulaAux.IndexOf("pv"), 3);
        }
        catch (ArgumentOutOfRangeException)
        {
            return 0;
        }
        string valor;
        switch (subCadena.Last().ToString())
        {
            case "i": valor = this.perfilViga.momentoDeInercia.ToString();
                break;
            default:
                throw new VarNotFoundInFormula();
        }
        this.formulaAux = this.formulaAux.Replace(subCadena, valor);
        return 1;
    }

#endregion

#endregion

    #region //Metodos principales
    /**
     * @param fuerzaYDistancia 
     * @param perfilViga       
     * @param modulo           
     * @return
     */
    private async System.Threading.Tasks.Task CalcularDeflexionAsync(List<FuerzaYDistancia> fuerzaYDistancia, PerfilViga perfilViga, Modulo modulo, string tipoDiagama)
    {
        Windows.Storage.StorageFile archivo = await BuscarArchivoAsync("formulariodedeflexionesparacalculoportablas.csv");
        List<string[]> texto = await LeerArchivoAsync(archivo);
        string[] formulasYDatos = await BuscarRenglonDeFormulasAsync(texto, tipoDiagama);
        //Guarda la ruta del diagrama que necesitará la interfaz, la ruta está en el archivo
        GuardarRutaDiagrama(formulasYDatos[1]);
        ConvertirFormularioAFormulas(formulasYDatos);
        this.deflexiones = new List<double>();
        foreach (string formula in this.formulas)
        {
            try
            {
                Expression e = new Expression(formula);
                double resultado = (double) e.Evaluate();
                deflexiones.Add(resultado);
            }
            catch (NCalc.EvaluationException e)
            {
                throw new ArithmeticException("Hubo un error al operar los datos", e);
            }
        }

    }

    /**
     * @param fuerzasydistancias 
     * @return
     */
    private void CalcularReaccion(List<FuerzaYDistancia> fuerzasydistancias)
    {

    }

    /**
     * @param fuerzasydistancias 
     * @return
     */
    private void CalcularMomento(List<FuerzaYDistancia> fuerzasydistancias)
    {
        // TODO implement here
    }
#endregion

}