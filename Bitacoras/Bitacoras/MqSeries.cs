using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//pruebas
using System.Configuration;

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

        //Variables prueba***************************
        public MQQueueManager QMGR = null;
        public MQQueueManager QMGR1 = null;
        public MQQueue QUEUE = null;
        public MQQueue QUEUE1 = null;
        public MQPutMessageOptions pmo = null;
        public MQMessage MSG = null;

        //*******************************************

        public MqSeries()
        {
        }

        /// <summary>
        /// Conectar al MQ
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
                Escribe("Conectado satisfactoriamente : " + QMGR.Name, "Mensaje");

                ConectarMQ = true;
            }
            catch (MQException mq_ex)
            {
                ConectarMQ = false;
                Escribe(mq_ex, "Error");
            }
            catch (Exception ex)
            {
                ConectarMQ = false;
                Escribe(ex, "Error");
            }


            return ConectarMQ;
        }

        /// <summary>
        /// Abrir cola del MQ
        /// </summary>
        /// <returns></returns>
        public bool AbrirColaMQ(MQQueueManager objMQManager, string strMQCola, MQQueue objMQCola, MQOPEN lngOpciones)
        {
            bool AbriColaMQ;
            try
            {
                //QMGR = new MQQueueManager("usemq");
                //QUEUE = QMGR.AccessQueue("SYSTEM.DEFAULT.LOCAL.QUEUE",
                //    MQC.MQOO_INPUT_SHARED +
                //    MQC.MQOO_OUTPUT +
                //    MQC.MQOO_BROWSE
                //);

                QUEUE = QMGR.AccessQueue(strMQCola,
                   (int)lngOpciones
                );

                AbriColaMQ = true;
            }
            catch (MQException mq_ex)
            {
                AbriColaMQ = false;
                Escribe(mq_ex, "Error");
            }
            catch (Exception ex)
            {
                AbriColaMQ = false;
                Escribe(ex, "Error");
            }

            return AbriColaMQ;
            
        }

        /// <summary>
        /// Enviar mensaje a la cola MQ
        /// </summary>
        /// <returns></returns>
        public bool EnviarMensajeMQ(MQQueueManager objMQManager, string strMQCola, MQQueue objMQCola, MQMessage objMQMensaje, string ls_mensaje, string Ls_ReplayMQQueue, string strMensajeID = "")
        {
            long lngMqOpen;
            bool EnviarMensajeMQ = false; 
            try
            {
                Escribe($"Funcion EnviarMensajeMQ:{QMGR.Name}", "Mensaje");
                //pmo = new MQPutMessageOptions();
                //pmo.Options = MQC.MQPMO_SYNCPOINT;
                //MSG = new MQMessage();
                //QUEUE.Put(MSG, pmo);
                //
                lngMqOpen = (long)MQOPEN.MQOO_OUTPUT;

                if (AbrirColaMQ(objMQManager, strMQCola, objMQCola, (MQOPEN)lngMqOpen))
                {
                    pmo = new MQPutMessageOptions();
                    pmo.Options = MQC.MQPMO_SYNCPOINT;
                    MSG = new MQMessage();
                    QUEUE.Put(MSG, pmo);
                    EnviarMensajeMQ = true;
                }
               
               
            }
            catch(MQException mq_ex)
            {
                EnviarMensajeMQ = false;
                Escribe(mq_ex, "Error");
            }
            catch (Exception ex)
            {
                EnviarMensajeMQ = false;
                Escribe(ex, "Error");
            }
           

            return EnviarMensajeMQ;
        }

        /// <summary>
        /// Desconectar el MQ
        /// </summary>
        /// <returns></returns>
        public bool DesconectarMQ()
        {
            bool DesconectarMQ;
            try
            {
                QMGR.Disconnect();
                Escribe("Desconectado satisfactoriamente : " + queueManager.Name, "Mensaje");
                DesconectarMQ = true;
            }
            catch (MQException mq_ex)
            {
                DesconectarMQ = false;
                Escribe(mq_ex, "Error");
            }
            catch (Exception ex)
            {
                DesconectarMQ = false;
                Escribe(ex, "Error");
            }
            return DesconectarMQ;
        }

        //*****************************************************************************************************************

        public string MQConectar(string strQueueManagerName, MQQueueManager queueManager)
        {
            string MQConectar ="";

            try
            {
                queueManager = new MQQueueManager(strQueueManagerName);
                //Set objMQManager = objMQConexion.AccessQueueManager(strMQManager)
                MQConectar = "Connected Successfully : " + queueManager.Name;

                Escribe(MQConectar, "Mensaje");

                return MQConectar;
            }
            catch (MQException exp)
            {
                Escribe(exp, "Error");
                MQConectar = "Exception: " + exp.Message;
                return MQConectar;
            }
            catch (Exception ex)
            {
                MQConectar = ex.Message;
                Escribe(ex, "Error");
                return MQConectar;
            }         
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
            catch (MQException exp)
            {
                Escribe(exp, "Error");
                return MQDesconectar;
            }
            catch (Exception ex)
            {
                Escribe(ex, "Error");
                return MQDesconectar;
            }
           
        }

        public bool MQEnviarMsg(MQQueueManager objMQManager, string strMQCola, MQQueue objMQCola, MQMessage objMQMensaje, string ls_mensaje, string Ls_ReplayMQQueue, string strMensajeID = "")
        {
            MQPutMessageOptions mqsMQOpciones;
            bool MQEnviarMsg = false;

            try
            {
                if (MQAbrirCola(objMQManager, strMQCola, objMQCola, MQOPEN.MQOO_OUTPUT))
                {

                }
                return true;
            }
            catch (MQException exp)
            {
                Escribe(exp, "Error");
                return MQEnviarMsg;
            }
            catch (Exception ex)
            {
                Escribe(ex, "Error");
                return MQEnviarMsg;
            }
            
        }

        private bool MQAbrirCola(MQQueueManager objMQManager, string strMQCola, MQQueue objMQCola, MQOPEN lngOpciones)
        {
            bool MQAbrirCola = false;
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
                Escribe(exp, "Error");
                return MQAbrirCola;
            }
            catch (Exception ex)
            {
                Escribe(ex, "Error");
                return MQAbrirCola;
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
            catch (MQException exp)
            {
                Escribe(exp, "Error");
                return MQCerrarCola;
            }
            catch (Exception ex)
            {
                Escribe(ex, "Error");
                return MQCerrarCola;
            }           
        }
        /// <summary>
        /// escribe en el log
        /// </summary>
        /// <param name="vData"></param>
        public void Escribe(string vData, string tipo)
        {
            string seccion = "escribeArchivoLOG";
            //Archivo = strlogFilePath & Format(Now(), "yyyyMMdd") & "-" & strlogFileName
            //string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string docPath = @"C:\tmp\log\";
            //string docPath = "D:\\Procesos\\TestMonitorMQTKTNet\\Procesos\\Log\\";

            if (true)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(getValueAppConfig(seccion, "logFilePath"), getValueAppConfig(seccion, "logFileName")), append: true))
                {
                    vData = $"[{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")}]  {tipo}:  {vData}";
                    Console.WriteLine(vData);
                    outputFile.WriteLine(vData);
                }

            }
        }

        /// <summary>
        /// Obtiene valor del parametro dado desde el app.config
        /// </summary>
        /// <param name="section">Seccion donde buscara</param>
        /// <param name="value">Valor que buscas</param>
        /// <returns></returns>
        private string getValueAppConfig(string section, string key)
        {
            return ConfigurationManager.AppSettings[$"{section}.{key}"]; ;
        }

        /// <summary>
        /// escribe en el log
        /// </summary>
        /// <param name="vData"></param>
        public void Escribe(Exception ex, string tipo)
        {
            string vData;
            string seccion = "escribeArchivoLOG";
            //Archivo = strlogFilePath & Format(Now(), "yyyyMMdd") & "-" & strlogFileName
            //string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string docPath = @"C:\tmp\log\";
            //string docPath = "D:\\Procesos\\TestMonitorMQTKTNet\\Procesos\\Log\\";

            if (true)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(getValueAppConfig(seccion, "logFilePath"), getValueAppConfig(seccion, "logFileName")), append: true))
                {
                    vData = $"[{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")}] {(char)13}" +
                        $"*{tipo}:  {ex.Message} {(char)13}" +
                        $"*InnerException: {ex.InnerException} {(char)13}" +
                        $"*Source: {ex.Source}  {(char)13}" +
                        $"*Data: {ex.Data}  {(char)13}" +
                        $"*HelpLink: {ex.HelpLink}  {(char)13}" +
                        $"*TargetSite: {ex.TargetSite}  {(char)13}";
                    Console.Write(vData);
                    outputFile.WriteLine(vData);
                }

            }
        }

    }
}

