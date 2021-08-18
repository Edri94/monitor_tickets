using Bitacoras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Probando_Bitacora
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Bitacora bitacora = new Bitacora();
            try
            { 
                bitacora.ProcesarBitacora("D:\\TEMPORAL\\", "QMDCEDTK-QRT.CEDTK.ENVIO.MQD8-1-INLOGTDD");
            }
            catch (Exception ex)
            {
                bitacora.Escribe("ERROR Message: " + ex.Message + " Errror Data: " + ex.Data, "Error");
            }        
        }
    }
}
