using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoBremen.Excepciones
{
    class FormulaNotFoundInFormulario : Exception
    {
        public FormulaNotFoundInFormulario()
        {
        }
        public FormulaNotFoundInFormulario(string message)
        : base(message)
        {
        }

        public FormulaNotFoundInFormulario(string message, Exception inner)
           : base(message, inner)
        {
        }
    }
}
