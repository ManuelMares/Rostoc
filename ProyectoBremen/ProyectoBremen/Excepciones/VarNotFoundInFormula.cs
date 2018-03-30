using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoBremen.Excepciones
{
    class VarNotFoundInFormula : Exception
    {
        public VarNotFoundInFormula()
        {
        }
        public VarNotFoundInFormula(string message)
        : base(message)
        {
        }

        public VarNotFoundInFormula(string message, Exception inner)
           : base(message, inner)
        {
        }
    }
}
