using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migracion.Clases
{
    internal class t_histonove
    {
        public string cod_ant { get; set; }            // Tipo de documento (código)
        public string cod_act { get; set; }
        public string usua_reg { get; set; }
        public DateTime fech_reg { get; set; }
        public TimeOnly hora_reg { get; set; }
        public string empleado { get; set; }
        public double repo_soi { get; set; }
        public DateTime fech_camb { get; set; }
    }
}
