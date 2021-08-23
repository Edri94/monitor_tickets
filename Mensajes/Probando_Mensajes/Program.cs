using Mensajes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Probando_Mensajes
{
    class Program
    {
        static void Main(string[] args)
        {
            Mensaje mensaje = new Mensaje();
            //mensaje.ProcesarMensajes("D:\\TEMPORAL\\", "QMDCEDTK-QRT.CEDTK.ENVIO.MQD8-1-INAUTPU");
            //mensaje.ProcesarMensajes("D:\\TEMPORAL\\", "QMDCEDTK-QRT.CEDTK.ENVIO.MQD8-F-INAUTPU");
            mensaje.ProcesarMensajes("D:\\TEMPORAL\\", "QMDCEDTK-QRT.CEDTK.ENVIO.MQD8-A-INAUTPU");

        }
    }
}
