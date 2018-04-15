using NCalc;
using ProyectoBremen.Excepciones;
using ProyectoBremen.MétodoPorTablas;
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
    public CalculadorPorTablas(PerfilViga perfilViga, Modulo modulo, Primitivos primitivos, List<Momento> momentos, List<FuerzaYDistancia> fuerzasYDistancias, List<CargaDistribuida> cargasDistribuidas, string tipoDiagrama)
    {
        //Iniciación y declaración de variables
        this.perfilViga = perfilViga;
        this.modulo = modulo;
        this.fuerzasYDistancias = fuerzasYDistancias;
        this.cargasDistribuidas = cargasDistribuidas;
        this.tipoDiagrama = tipoDiagrama;
        this.momentos = momentos;
        this.primitivos = primitivos;

        LlamarMetodosAsync();

    }

    private async System.Threading.Tasks.Task LlamarMetodosAsync()
    {
        await CalcularDeflexionAsync(fuerzasYDistancias, perfilViga, modulo, tipoDiagrama);
        this.CalcularMomento(fuerzasYDistancias);
        this.CalcularReaccion(fuerzasYDistancias);
    }

    #region ATRIBUTOS
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
    private List<CargaDistribuida> cargasDistribuidas;

    /**
     * 
     */
    private List<double> deflexiones;

    /**
     * 
     */
    private List<Momento> momentos;

    /**
     * 
     */
    private Primitivos primitivos;

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

#endregion

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
     * Este método lee el archivo, de existir un problema con el archivo a buscar, lanza una excepción de tipo 'FileNotFoundException'
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
    /*
     * Recibe un renglón de fórmulas tipo string y lo convierte en fórmulas operables. Guarda las fórmulas de manera interna
     */
    private void ConvertirFormularioAFormulas(string[] contenido)
    {
        //CONTENIDO TIENE 2 FORMULAS Y POR ESO NO ITERA SOBRE LA LETRA, SINO SOBRE LAS FÓRMULAS DISPONIBLES
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
        while (estado != 0)
        {
            estado = ReemplazarCargaDistribuida();
        }
        while (estado != 0)
        {
            estado = ReemplazarMomentos();
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
        estado = 1;
        while (estado != 0)
        {
            estado = ReemplazarPrimitivos();
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
        else if (subCadena.ElementAt(1).ToString() == "m")
        {
            valor = this.fuerzasYDistancias[int.Parse(subCadena.Last().ToString())].magnitud.ToString();
        }
        else
        {
            throw new VarNotFoundInFormula();
        }
        this.formulaAux = this.formulaAux.Replace(subCadena, valor);
        return 1;
    }

    private int ReemplazarMomentos()
    {
        string subCadena;
        try
        {
            subCadena = this.formulaAux.Substring(this.formulaAux.IndexOf("n"), 3);
        }
        catch (ArgumentOutOfRangeException)
        {
            return 0;
        }
        string valor;
        if (subCadena.ElementAt(1).ToString() == "y")
        {
            valor = this.momentos[int.Parse(subCadena.Last().ToString())].magnitud.ToString();
        }
        else if (subCadena.ElementAt(1).ToString() == "b")
        {
            valor = this.momentos[int.Parse(subCadena.Last().ToString())].distancia.ToString();
        }
        else
        {
            throw new VarNotFoundInFormula();
        }
        this.formulaAux = this.formulaAux.Replace(subCadena, valor);
        return 1;
    }

    private int ReemplazarCargaDistribuida()
    {
        string subCadena;
        try
        {
            subCadena = this.formulaAux.Substring(this.formulaAux.IndexOf("c"), 3);
        }
        catch (ArgumentOutOfRangeException)
        {
            return 0;
        }
        string valor;
        if (subCadena.ElementAt(1).ToString() == "tq")
        {
            valor = this.cargasDistribuidas[int.Parse(subCadena.Last().ToString())].distanciaInicial.ToString();
        }
        else if (subCadena.ElementAt(1).ToString() == "pv")
        {
            valor = this.cargasDistribuidas[int.Parse(subCadena.Last().ToString())].distanciaFinal.ToString();
        }
        else if (subCadena.ElementAt(1).ToString() == "po")
        {
            valor = this.cargasDistribuidas[int.Parse(subCadena.Last().ToString())].magnitudInicial.ToString();
        }
        else if (subCadena.ElementAt(1).ToString() == "pu")
        {
            valor = this.cargasDistribuidas[int.Parse(subCadena.Last().ToString())].magnitudFinal.ToString();
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
            subCadena = this.formulaAux.Substring(this.formulaAux.IndexOf("e"), 1);
        }
        catch (ArgumentOutOfRangeException)
        {
            return 0;
        }
        string valor = "";
        valor = this.modulo.valor.ToString();

        this.formulaAux = this.formulaAux.Replace(subCadena, valor);
        return 1;
    }

    private int ReemplazarPerfilViga()
    {
        string subCadena;
        try
        {
            subCadena = this.formulaAux.Substring(this.formulaAux.IndexOf("i"), 1);
        }
        catch (ArgumentOutOfRangeException)
        {
            return 0;
        }
        string valor = "";
        this.perfilViga.momentoDeInercia.ToString();

        this.formulaAux = this.formulaAux.Replace(subCadena, valor);
        return 1;
    }

    private int ReemplazarPrimitivos()
    {
        string subCadena;
        try
        {
            subCadena = this.formulaAux.Substring(this.formulaAux.IndexOf("p"), 2);
        }
        catch (ArgumentOutOfRangeException)
        {
            return 0;
        }
        string valor;
        if (subCadena.ElementAt(1).ToString() == "a")
        {
            valor = this.primitivos.apoyo1.ToString();
        }
        else if (subCadena.ElementAt(1).ToString() == "s")
        {
            valor = this.primitivos.apoyo2.ToString();
        }
        else if (subCadena.ElementAt(1).ToString() == "l")
        {
            valor = this.primitivos.longitud.ToString();
        }
        else
        {
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