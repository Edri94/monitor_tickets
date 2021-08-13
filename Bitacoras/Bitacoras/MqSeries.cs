using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitacoras
{
    public class MqSeries
    {
        public string strlogFilePath;
        public string strlogFileName;
        public bool Mb_GrabaLog;

        private string Archivo;


        // Enumeración para las opciones de abrir la cola
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
        }

        // Enumeración para el tipo de acción
        enum TipoAccion
        {
            eMQConectar = 0,
            eMQDesconectar = 1,
            eMQAbrirCola = 2,
            eMQCerrarCola = 3,
            eMQLeerCola = 4,
            eMQEscribirCola = 5,
            eMQOtro = 6
        }

        // Variable para validar la conexión
        public bool blnConectado;

        // ******************************************************************************************
        // Variables y objectos publicos para conectarse al MQSeries

        //// Declaraciones de los objetos para MQSeries
        //// Referencia: IBM MQSeries Automation Classes for ActiveX
        ////public mqSession mqSession = new mqSession();           // Objeto Session para conexión con el servidor MQSeries
        //public MQQueueManager mqManager = new MQQueueManager();      // -Objeto QueueManager para accesar al maestro de colas
        //public MQQueue mqsEscribir;             // -Objeto Queue para escribir
        //public MQQueue mqsLectura;             // -Objeto Queue para lectura
        //public MQMessage mqsMsgEscribir = new MQMessage();           // -Objeto Message para escribir
        //public MQMessage mqsMsglectura = new MQMessage();           // -Objeto Message para lectura

        //Declaraciones de los objetos para MQSeries
        public MQQueueManager queueManager;
        public MQQueue queue;
        public MQMessage queueMessage;
        public MQPutMessageOptions queuePutMessageOptions;
        public MQGetMessageOptions queueGetMessageOptions;

        private const String connectionType = MQC.TRANSPORT_MQSERIES_CLIENT;

        private string QueueManagerName;

     
        public string MQConectar(string strQueueManagerName, MQQueueManager queueManager)
        {
            string strReturn;
  
            try
            {
                queueManager = new MQQueueManager(strQueueManagerName);
                //Set objMQManager = objMQConexion.AccessQueueManager(strMQManager)
                strReturn = "Connected Successfully : " + queueManager.Name;
                
                Escribe(strReturn);
            }
            catch (MQException exp)
            {

                //string strError = getMQRCText(exp.Reason);
                Escribe("Conecta MQ ExcepcionMQ Error trying to create Queue" + "Manager Object. Error Message: " + exp.Message + ", Reason: " + exp.Reason + ", ReasonCode: " + exp.ReasonCode);
                strReturn = "Exception: " + exp.Message;
                //Escribe("");
            }

            return strReturn;
        }


        public bool MQDesconectar(MQQueueManager objMQManager, MQQueue objMQEscribir)
        {
            bool MQDesconectar = false;
            try
            {
                if (objMQEscribir != null)
                {
                    if (objMQEscribir.IsOpen)
                    {
                        objMQEscribir = null;
                    }
                }

                if (objMQManager != null)
                {
                    if (objMQManager.IsConnected)
                    {
                        objMQManager.Disconnect();
                        objMQManager = null;
                        MQDesconectar = true;
                    }
                }

                //Set mqSession = Nothing

                return MQDesconectar;
            }
            catch (Exception ex)
            {
                Escribe(ex.Message);
                return MQDesconectar;
            }
        }

        public bool MQEnviarMsg(MQQueueManager objMQManager, string strMQCola, MQQueue objMQCola, MQMessage objMQMensaje, string ls_mensaje, string Ls_ReplayMQQueue, string strMensajeID = "")
        {
            MQPutMessageOptions mqsMQOpciones;
            string strMensaje;

            try
            {
                //Set mqsMQOpciones = objMQConexion.AccessPutMessageOptions
                //mqsMQOpciones.Options = mqsMQOpciones.Options Or MQPMO_NO_SYNCPOINT
                //Set objMQMensaje = objMQConexion.AccessMessage

                if (MQAbrirCola(objMQManager, strMQCola, objMQCola, MQOPEN.MQOO_OUTPUT))
                {

                }
                return true;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private bool MQAbrirCola(MQQueueManager objMQManager, string strMQCola, MQQueue objMQCola, MQOPEN lngOpciones)
        {
            try
            {

                //MQParaLectura = objMQManager.AccessQueue(strMQCola, (int)lngOpciones);
                ////' Se accesa la cola ya sea para leer o escribir
                //Set objMQCola = objMQManager.AccessQueue(strMQCola, lngOpciones, mqManager.Name, "AMQ.*")
                //MQAbrirCola = True
                return true;
            }
            catch (MQException exp)
            {
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
           
        }

        public bool MQCerrarCola(MQQueue objMCola)
        {
            bool MQCerrarCola = false;
            try
            {
                if (objMCola != null)
                {
                    if (objMCola.IsOpen)
                    {
                        objMCola.Close();
                        MQCerrarCola = true;
                    }
                }
                return MQCerrarCola;
            }
            catch (Exception ex)
            {
                Escribe(ex.Message);
                return MQCerrarCola;
            }
        }


        /// <summary>
        /// escribe en el log
        /// </summary>
        /// <param name="vData"></param>
        public void Escribe(string vData)
        {
            //Archivo = strlogFilePath & Format(Now(), "yyyyMMdd") & "-" & strlogFileName
            //string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string docPath = @"C:\tmp\log\";
            string docPath = "D:\\Procesos\\TestMonitorMQTKTNet\\Procesos\\Log\\";

            if (true)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "log.txt"), append: true))
                {
                    vData = "[" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + "]  Error: " + vData;
                    Console.WriteLine(vData);
                    outputFile.WriteLine(vData);
                }

            }
        }
    }
}
