using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorMQTKT.Models
{
    public class Autorizacion
    {
        public string FuncionSQL { get; set; }
        public string FechaProce { get; set; }
        public string HoraProce { get; set; }
        public string DatosTemp { get; set; }
        public string Espera { get; set; }
    }
}
