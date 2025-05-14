using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migracion.Clases
{
    internal class t_vended
    {
        public string Vended { get; set; } // Código del vendedor
        public string Nombre { get; set; }
        public int Cedula { get; set; }
        public string Tel { get; set; }
        public string Direcc { get; set; }
        public string Ciudad { get; set; }
       
        public DateTime FechIng { get; set; }

        public string Ccosto { get; set; }
    }
}
