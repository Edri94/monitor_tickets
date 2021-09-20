using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MonitorMQTKT.Funciones
{
    public class MqSeries
    {
        private static readonly TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
        private static readonly MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();

        private static readonly string strArchivoIni;
        public string ArchivoLog;
        public static string dateFormat = "yyyyMMdd";
        public static string dateFormatHora = "yyyy-MM-dd HHmmss";
        public static string currentDate;
        public static DateTime date;

        public static bool bIniciarmod;
        public static bool bMQConectar;
        public static bool bMQAbrirCola;
        public static bool bfValidaEjecucion;
        public static bool bReConectar;
        public static double dblRevisaMQ;

        public enum MQOPEN
        {
            MQOO_INPUT_AS_Q_DEF = 0x1,
            MQOO_INPUT_SHARED = 0x2,
            MQOO_INPUT_EXCLUSIVE = 0x4,
            MQOO_BROWSE = 0x8,
            MQOO_OUTPUT = 0x10,
            MQOO_INQUIRE = 0x20,
            MQOO_SET = 0x40,
            MQOO_BIND_ON_OPEN = 0x4000,
            MQOO_BIND_NOT_FIXED = 0x8000,
            MQOO_BIND_AS_Q_DEF = 0x0,
            MQOO_SAVE_ALL_CONTEXT = 0x80,
            MQOO_PASS_IDENTITY_CONTEXT = 0x100,
            MQOO_PASS_ALL_CONTEXT = 0x200,
            MQOO_SET_IDENTITY_CONTEXT = 0x400,
            MQOO_SET_ALL_CONTEXT = 0x800,
            MQOO_ALTERNATE_USER_AUTHORITY = 0x1000,
            MQOO_FAIL_IF_QUIESCING = 0x2000
        };

        public enum MQE
        {
            MQTRANSPORT_PROPERTY = '1',
            MQHOST_NAME_PROPERTY = '0',
            MQPORT_PROPERTY = 1414,
            MQCHANNEL_PROPERTY = '0',
        };


        public MQQueueManager QMGR = null; //mqsManager
        public MQQueueManager QMGR1 = null;
        public MQQueue QUEUE = null; //MQQMonitorLectura
        public MQQueue QUEUE1 = null;
        public MQPutMessageOptions pmo = null;
        public MQMessage MSG = null;


        //' Variables para controlar los periodos de monitoreo
        public  string FechaRestar;                      //' Valor que determina la fecha y hora para ejecutar un restar del monitor
        public  string StrLogPath;                       //' Ruta para almacenar el log del monitor
        public  string StrlogName;                       //' Nombre y extención del archivo log
        public  string StrlogActivo;                     //' Nombre y extención del archivo log

  

        public double dblCiclosBitacoras;
        public double dblCiclosTKTMQ;
        public double dblCiclosFuncionarios;
        public double dblCiclosAutorizaciones;

        public string strMQManager;
        public string strMQQMonitorEscritura;
        public string strMQQMonitorLectura;

        public int intgModoMonitor;
        public int intgActv_FuncAuto;
        public int intgMonitor;

        public string inttmrRestar;
        public string inttmrMonitor;
        public string inttmrBitacora;
        
        public int intgtmrRestar;
        public int intgtmrMonitor;
        public int intgtmrBitacora;


        //private string[] Temporal = TiempoBitacoras.Split(',');
        public string strFormatoTiempoBitacoras;
        public string strFormatoTiempoTKTMQ;
        public string strFormatoTiempoFuncionarios;
        public string strFormatoTiempoAutorizaciones;

        public string TiempoBitacoras;
        public string TiempoMensajes;
        public string Tiempofuncionarios;
        public string TiempoAutorizacion;

        public int intTiempoBitacoras;
        public int intTiempoTKTMQ;
        public int intTiempoFuncionarios;
        public int intTiempoAutorizaciones;

        public static string RestarMonitor;
        public static string LogPath;
        public static string LogFile ;
        public static string LogActivo ;

        public string PMONITOREOS;
        public string PARAMETRO1;
        public string PARAMETRO2;
        public string PARAMETRO3;
        public string PARAMETRO4;

        public string PROCESOS;
        public string PROCESO1;
        public string PROCESO2;
        public string PROCESO3;
        public string PROCESO4;
        public string PROCESO5;
        public string PROCESO6;

        public string Autorizaciones_Operatoria;
        public string Log_Apertura_de_Cuentas;
        public string LOG_Ordenes_de_Pago;
        public string LOG_Operaciones_CED;
        public string LOG_Operaciones_TDD;

        public string Ambiente;
        public string Prefijo;
        public string fileFuncionarios;
        public string fileSucursales;
        public string fileUsuarios;

        public string dirBitacora ;
        public string Bitacora ;


        public bool blnConectado;

        public Funcion funcion;


        public MqSeries()
        {
            funcion = new Funcion();

            strMQManager = funcion.getValueAppConfig("MQManager", "");
            strMQQMonitorEscritura = funcion.getValueAppConfig("MQEnvioMsgMonitor", "");
            strMQQMonitorLectura = funcion.getValueAppConfig("MQRecepResMonitor", "");

            intgModoMonitor = Convert.ToInt32(funcion.getValueAppConfig("intModoMonitor", "0"));
            intgActv_FuncAuto = Convert.ToInt32(funcion.getValueAppConfig("intgActv_FuncAuto", "0"));
            intgMonitor = Convert.ToInt32(funcion.getValueAppConfig("intMonitor", "0"));

            inttmrRestar = funcion.getValueAppConfig("inttmrRestar", "");
            inttmrMonitor = funcion.getValueAppConfig("inttmrMonitor", "");
            inttmrBitacora = funcion.getValueAppConfig("inttmrBitacora", "");

            TiempoBitacoras = funcion.getValueAppConfig("TiempoBitacoras", "");
            TiempoMensajes = funcion.getValueAppConfig("TiempoMensajes", "");
            Tiempofuncionarios = funcion.getValueAppConfig("Tiempofuncionarios", "");
            TiempoAutorizacion = funcion.getValueAppConfig("TiempoAutorizacion", "");

            RestarMonitor = funcion.getValueAppConfig("RestarMonitor", "");
            LogPath = funcion.getValueAppConfig("escribeArchivoLOG.LogPath", "");
            LogFile = funcion.getValueAppConfig("escribeArchivoLOG.LogFile", "");
            LogActivo = funcion.getValueAppConfig("LogActivo", "");

            PMONITOREOS = funcion.getValueAppConfig("PMONITOREOS", "");
            PARAMETRO1 = funcion.getValueAppConfig("PARAMETRO1", "");
            PARAMETRO2 = funcion.getValueAppConfig("PARAMETRO2", "");
            PARAMETRO3 = funcion.getValueAppConfig("PARAMETRO3", "");
            PARAMETRO4 = funcion.getValueAppConfig("PARAMETRO4", "");

            PROCESOS = funcion.getValueAppConfig("PROCESOS", "");
            PROCESO1 = funcion.getValueAppConfig("PROCESO1", "");
            PROCESO2 = funcion.getValueAppConfig("PROCESO2", "");
            PROCESO3 = funcion.getValueAppConfig("PROCESO3", "");
            PROCESO4 = funcion.getValueAppConfig("PROCESO4", "");
            PROCESO5 = funcion.getValueAppConfig("PROCESO5", "");
            PROCESO6 = funcion.getValueAppConfig("PROCESO6", "");

            Autorizaciones_Operatoria = funcion.getValueAppConfig("Autorizaciones Operatoria", "");
            Log_Apertura_de_Cuentas = funcion.getValueAppConfig("Log Apertura de Cuentas", "");
            LOG_Ordenes_de_Pago = funcion.getValueAppConfig("LOG Ordenes de Pago", "");
            LOG_Operaciones_CED = funcion.getValueAppConfig("LOG Operaciones CED", "");
            LOG_Operaciones_TDD = funcion.getValueAppConfig("LOG Operaciones TDD", "");

            Ambiente = funcion.getValueAppConfig("Ambiente", "TEST");
            Prefijo = funcion.getValueAppConfig("Prefijo", "P_");
            fileFuncionarios = funcion.getValueAppConfig("fileFuncionarios", "PFuncionarios.txt");
            fileSucursales = funcion.getValueAppConfig("fileSucursales", "PSucursales.txt");
            fileUsuarios = funcion.getValueAppConfig("fileUsuarios", "PUsuarios.txt");

            dirBitacora = funcion.getValueAppConfig("RutaBitacora", "");
            Bitacora = funcion.getValueAppConfig("Bitacora", "");


        }

        /// <summary>
        /// Cerrar cola MqSeries
        /// </summary>
        /// <returns></returns>
        public bool CerrarColaMQ()
        {
            try
            {
                if(QUEUE != null)
                {
                    if(QUEUE.IsOpen)
                    {
                        QUEUE.Close();
                        return true;
                    }
                }
                return false;
            }
            catch (MQException ex)
            {
                funcion.Escribe(ex, "MQException");
                return false;
            }
            catch (Exception ex)
            {
                funcion.Escribe(ex);
                return false;
            }
        }

        /// <summary>
        /// Desconectar el MqSeries
        /// </summary>
        /// <returns></returns>
        public bool DesconectarMQ()
        {
            bool DesconectarMQ;
            try
            {
                QMGR.Disconnect();
                funcion.Escribe("Desconectado satisfactoriamente : " + QMGR.Name);
                DesconectarMQ = true;
            }
            catch (MQException mq_ex)
            {
                DesconectarMQ = false;
                funcion.Escribe(mq_ex);
            }
            catch (Exception ex)
            {
                DesconectarMQ = false;
                funcion.Escribe(ex);
            }
            return DesconectarMQ;
        }

        /// <summary>
        /// Conectar al MqSeries
        /// </summary>
        /// <param name="strQueueManagerName"></param>
        /// <param name="queueManager"></param>
        /// <returns></returns>
        public bool ConectarMQ(string strQueueManagerName)
        {
            bool ConectarMQ;
            try
            {
                //QMGR = new MQQueueManager("usemq");
                QMGR = new MQQueueManager(strQueueManagerName);
                funcion.Escribe("Conectado satisfactoriamente : " + QMGR.Name);

                ConectarMQ = true;
            }
            catch (MQException mq_ex)
            {
                ConectarMQ = false;
                funcion.Escribe(mq_ex);
            }
            catch (Exception ex)
            {
                ConectarMQ = false;
                funcion.Escribe(ex, "Error");
            }


            return ConectarMQ;
        }


    }
}
