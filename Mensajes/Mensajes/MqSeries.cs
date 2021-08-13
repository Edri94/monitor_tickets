using ConexionBDSQL;
using IBM.WMQ;
using System;
using System.IO;

namespace Mensajes
{
    public class MqSeries
    {
        public string gstrRutaIni;
        public ConexionBD cnnConexion;
        public string[] rssRegistro;

        public string gsPswdDB;
        public string gsUserDB;
        public string gsNameDB;
        public string gsCataDB;
        public string gsDSNDB;

        public string strQuery;           // Cadena para almacenar el Query a ejecutarse en la base de datos

        public string gsAccesoActual;               // Fecha/Hora actual del sistema. La tomamos del servidor NT y no de SQL porque precisamente el
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

        // Declaraciones de los objetos para MQSeries
        // Referencia: IBM MQSeries Automation Classes for ActiveX
        //public mqSession mqSession = new mqSession();             // Objeto Session para conexión con el servidor MQSeries
        public MQQueueManager mqManager;                            // -Objeto QueueManager para accesar al maestro de colas
        public MQQueue mqsEscribir;                                 // -Objeto Queue para escribir
        public MQQueue mqsLectura;                                  // -Objeto Queue para lectura
        public MQMessage mqsMsgEscribir;                            // -Objeto Message para escribir
        public MQMessage mqsMsglectura;                             // -Objeto Message para lectura


        public MqSeries()
        {
            mqManager = new MQQueueManager();
            //mqsEscribir = new MQQueue();
            //mqsLectura = new MQQueue();
            mqsMsgEscribir = new MQMessage();
            mqsMsglectura = new MQMessage();
        }

        public bool MQConectar(string strMQManager, MQQueueManager objMQManager)
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }

        public bool MQDesconectar(MQQueueManager objMQManager, MQQueue objMQEscribir)
        {
            try
            {
                if (objMQManager != null)
                {
                    if (objMQManager.IsConnected)
                    {
                        objMQManager.Disconnect();
                        objMQManager = null;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }

        public bool MQAbrirCola(MQQueueManager objMQManager, string strMQCola, MQQueue objMQCola, MQOPEN lngOpciones)
        {
            try
            {
                objMQCola = objMQManager.AccessQueue(strMQCola, 0);
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }

        public bool MQCerrarCola(MQQueue objMQCola)
        {
            try
            {
                if (objMQCola != null)
                {
                    if (objMQCola.IsOpen)
                    {
                        objMQCola.Close();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }

        public string VerificarMQQueue(MQQueueManager objMQManager, string strMQCola, MQQueue objMQCola, string lngOpciones)
        {
            try
            {
                objMQCola = objMQManager.AccessQueue(strMQCola, 0);
                objMQCola.Close();

            }
            catch (Exception)
            {

                throw;
            }
            return "";

        }

        public bool MQEnviarMsg(MQQueueManager objMQManager, string strMQCola, MQQueue objMQCola, MQMessage objMQMensaje, string ls_mensaje, string Ls_ReplayMQQueue, string psPersistencia, string psExpirar, string strMensajeID = "")
        {
            try
            {
                MQPutMessageOptions mqsMQOpciones = new MQPutMessageOptions();
                string strMensaje;

                //Set mqsMQOpciones = objMQConexion.AccessPutMessageOptions
                //mqsMQOpciones.Options = mqsMQOpciones.Options Or MQPMO_NO_SYNCPOINT
                //Set objMQMensaje = objMQConexion.AccessMessage

                if (MQAbrirCola(objMQManager, strMQCola, objMQCola, MQOPEN.MQOO_OUTPUT))
                {
                    strMensaje = ls_mensaje;
                    objMQMensaje.ClearMessage();
                    objMQMensaje.Format = "MQSTR";
                    objMQMensaje.MessageType = 0;
                    objMQMensaje.WriteLong(strMensaje.Trim().Length);
                    //objMQMensaje.MessageData = Trim(strMensaje)

                    //objMQMensaje.Persistence = psPersistencia
                    //objMQMensaje.Expiry = psExpirar

                    objMQMensaje.ReplyToQueueManagerName = Ls_ReplayMQQueue;
                    objMQCola.Put(objMQMensaje, mqsMQOpciones);

                    MQCerrarCola(objMQCola);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }

        public void psInsertarSQL(string psFechaHora, int pnNumeroError, string psDescripcion, string psAplicacion, string psFuncion)
        {
            strQuery = "Insert into " + gsNameDB + "..BITACORA_ERRORES_MENSAJES_PU ";
            strQuery += "(fecha_hora, error_numero, error_descripcion, aplicacion) ";
            strQuery += "Values ('" + DateTime.ParseExact(psFechaHora, "yyyy-mm-dd hh:mm:ss", null) + "', " + pnNumeroError + ", '" + psDescripcion + "', '" + psAplicacion + "')";

        }

        public void Escribe(string vData)
        {
            //Archivo = strlogFilePath & Format(Now(), "yyyyMMdd") & "-" & strlogFileName
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (Mb_GrabaLog)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "")))
                {
                    Console.WriteLine(vData);
                    outputFile.WriteLine(vData);
                }

            }
        }

    }
}
