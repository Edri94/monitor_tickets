using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorMQTKT.Funciones
{
    public class MqMonitorTicket : MqSeries
    {
        public DateTime date;
        public DateTime currentDate;
        Funcion funcion;
        public MqMonitorTicket()
        {
            funcion = new Funcion();
            date = DateTime.Now;
            currentDate = DateTime.Now;
        }
        public bool Inicia()
        {
            //falta validación de entradas
            bool inicia;

            funcion.Escribe("", "entro as Inicia");

            //ModFunciones.date = DateTime.Now;
            //currentDate = ModFunciones.date.ToString(dateFormat);

            strMQManager = funcion.getValueAppConfig("MQManager", "");
            strMQQMonitorEscritura = funcion.getValueAppConfig("MQEnvioMsgMonitor", "");
            strMQQMonitorLectura = funcion.getValueAppConfig("MQRecepResMonitor", "");

            intgModoMonitor = Convert.ToInt32(funcion.getValueAppConfig("intModoMonitor", "0"));
            intgActv_FuncAuto = Convert.ToInt32(funcion.getValueAppConfig("intgActv_FuncAuto", "0"));
            intgMonitor = Convert.ToInt32(funcion.getValueAppConfig("intMonitor", "0"));

            intgtmrRestar = Convert.ToInt32(inttmrRestar);
            intgtmrMonitor = Convert.ToInt32(inttmrMonitor);
            intgtmrBitacora = Convert.ToInt32(inttmrBitacora);

            TiempoBitacoras = funcion.getValueAppConfig("TiempoBitacoras", "");

            string[] vs = TiempoBitacoras.Split(',');
            string[] Temporal = vs;
            string strFormatoTiempoBitacoras = Temporal[0];
            intTiempoBitacoras = Convert.ToInt32(Temporal[1]);

            TiempoMensajes = funcion.getValueAppConfig("TiempoMensajes", "");
            vs = TiempoMensajes.Split(',');
            Temporal = vs;
            string strFormatoTiempoTKTMQ = Temporal[0];
            intTiempoTKTMQ = Convert.ToInt32(Temporal[1]);

            Tiempofuncionarios = funcion.getValueAppConfig("Tiempofuncionarios", "");
            vs = Tiempofuncionarios.Split(',');
            Temporal = vs;
            string strFormatoTiempoFuncionarios = Temporal[0];
            intTiempoFuncionarios = Convert.ToInt32(Temporal[1]);

            TiempoAutorizacion = funcion.getValueAppConfig("TiempoAutorizacion", "");
            vs = TiempoAutorizacion.Split(',');
            Temporal = vs;
            string strFormatoTiempoAutorizaciones = Temporal[0];
            intTiempoAutorizaciones = Convert.ToInt32(Temporal[1]);

            FechaRestar = RestarMonitor;
            StrlogName = LogFile;
            StrLogPath = LogPath;
            StrlogActivo = LogActivo;

            if (String.IsNullOrEmpty(strMQManager))
            {
                inicia = false;
            }
            else
            {
                inicia = true;
            }

            return inicia;
        }
    }
}
