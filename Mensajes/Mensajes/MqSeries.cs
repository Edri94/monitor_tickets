using ConexionBDSQL;
using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
        public string gsSrvr;

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


        //// ******************************************************************************************
        //// Variables y objectos publicos para conectarse al MQSeries

        //// Declaraciones de los objetos para MQSeries
        //// Referencia: IBM MQSeries Automation Classes for ActiveX
        ////public mqSession mqSession = new mqSession();             // Objeto Session para conexión con el servidor MQSeries
        //public MQQueueManager mqManager;                            // -Objeto QueueManager para accesar al maestro de colas
        //public MQQueue mqsEscribir;                                 // -Objeto Queue para escribir
        //public MQQueue mqsLectura;                                  // -Objeto Queue para lectura
        //public MQMessage mqsMsgEscribir;                            // -Objeto Message para escribir
        //public MQMessage mqsMsglectura;

        public MQQueueManager queueManager;
        public MQQueue queue;
        public MQMessage queueMessage;
        public MQPutMessageOptions queuePutMessageOptions;
        public MQGetMessageOptions queueGetMessageOptions;



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

        //**************************************************************************************************************
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
                Escribe("Abriendo Cola", "Mensaje");
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
            catch (MQException mq_ex)
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
                Escribe("Me voy a desconectar del MQ", "Mensaje");
                QMGR.Disconnect();
                Escribe("Desconectado satisfactoriamente : " + QMGR.Name, "Mensaje");
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

        /// <summary>
        /// Ejecutar una consulta con un query dado
        /// </summary>
        /// <param name="query">query select</param>
        /// <returns></returns>
        private SqlDataReader ejecutarConsulta(string query)
        {
            try
            {
                cnnConexion.ActiveConnection = true;
                cnnConexion.ParametersContains = false;
                cnnConexion.CommandType = CommandType.Text;
                cnnConexion.ActiveConnection = true;

                SqlDataReader sqlRecord = cnnConexion.ExecuteDataReader(query);

                return sqlRecord;
            }
            catch (Exception ex)
            {
                return null;
                throw;
            }
        }

        /// <summary>
        /// Obtiene un datatable con la informacion del query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataTable ConsultaMQQUEUEFunc(string query)
        {
            try
            {
                SqlDataReader dr = this.ejecutarConsulta(query);

                DataTable dt = new DataTable();
                
                dt.Clear();
                
                dt.Columns.Add("id_funcionario");
                dt.Columns.Add("centro_regional");
                dt.Columns.Add("numero_funcionario");
                dt.Columns.Add("producto");
                dt.Columns.Add("subproducto");
                dt.Columns.Add("fecha_alta");
                dt.Columns.Add("fecha_baja");
                dt.Columns.Add("fecha_ultimo_mant");
                dt.Columns.Add("tipo_peticion");
                dt.Columns.Add("status_envio");
                dt.Columns.Add("columna_11");
                dt.Columns.Add("columna_12");
                dt.Columns.Add("id_transaccion");
                dt.Columns.Add("tipo");


                if (dr != null)
                {
                    while (dr.Read())
                    {
                        DataRow _row = dt.NewRow();

                        _row["id_funcionario"] = dr.GetInt32(0);
                        _row["centro_regional"] = dr.GetString(1);
                        _row["numero_funcionario"] = dr.GetString(2);
                        _row["producto"] = dr.GetString(3);
                        _row["subproducto"] = dr.GetString(4);
                        _row["fecha_alta"] = dr.GetString(5);
                        _row["fecha_baja"] = dr.GetString(6);
                        _row["fecha_ultimo_mant"] = dr.GetString(7);
                        _row["tipo_peticion"] = dr.GetString(8);
                        _row["status_envio"] = dr.GetByte(9);
                        _row["columna_11"] = dr.GetString(10);
                        _row["columna_12"] = dr.GetString(11);
                        _row["id_transaccion"] = dr.GetInt32(12);
                        _row["tipo"] = dr.GetString(13);

                        dt.Rows.Add(_row);
                    }
                    return dt;
                }
            }
            catch (SqlException ex)
            {
                Escribe(ex, "Error");
            }
            catch (Exception ex)
            {
                Escribe(ex, "Error");
            }
            return null;
        }

        public DataTable ConsultaMQQUEUEAuto(string query)
        {
            try
            {
                SqlDataReader dr = this.ejecutarConsulta(query);

                DataTable dt = new DataTable();
                dt.Clear();

                dt.Columns.Add("operacion");
                dt.Columns.Add("oficina");
                dt.Columns.Add("numero_funcionario");
                dt.Columns.Add("id_transaccion");
                dt.Columns.Add("codigo_operacion");
                dt.Columns.Add("cuenta");
                dt.Columns.Add("divisa");
                dt.Columns.Add("importe");
                dt.Columns.Add("fecha_operacion");
                dt.Columns.Add("folio_autorizacion");
                dt.Columns.Add("status_envio");
                dt.Columns.Add("fecha");
                dt.Columns.Add("hora");


                if (dr != null)
                {
                    while (dr.Read())
                    {
                        DataRow _row = dt.NewRow();

                        _row["operacion"] = dr.GetInt32(0);
                        _row["oficina"] = dr.GetString(1);
                        _row["numero_funcionario"] = dr.GetString(2);
                        _row["id_transaccion"] = dr.GetString(3);
                        _row["codigo_operacion"] = dr.GetString(4);
                        _row["cuenta"] = dr.GetString(5);
                        _row["divisa"] = dr.GetString(6);
                        _row["importe"] = dr.GetString(7);
                        _row["fecha_operacion"] = dr.GetString(8);
                        _row["folio_autorizacion"] = dr.GetString(9);
                        _row["status_envio"] = dr.GetByte(10);
                        _row["fecha"] = dr.GetString(11);
                        _row["hora"] = dr.GetString(12);

                        dt.Rows.Add(_row);
                    }
                    return dt;
                }
            }
            catch (SqlException ex)
            {
                Escribe(ex, "Error");
            }
            catch (Exception ex)
            {
                Escribe(ex, "Error");
            }
            return null;
        }
        /// <summary>
        /// Obtener lista de errores a partir de la fecha dada, desde la tabla BITACORA_ERRORES_MENSAJES_PU
        /// </summary>
        /// <param name="fecha">a partir de que fecha</param>
        /// <param name="hora">y apartir de que hora</param>
        /// <returns></returns>
        public List<Bitacora_Errores_Mensajes_Pu> ConsultaBitacoraErroresMensajes(string fecha, string hora)
        {
            List<Bitacora_Errores_Mensajes_Pu> list = new List<Bitacora_Errores_Mensajes_Pu>();
            try
            {
                string query = $"select * from BITACORA_ERRORES_MENSAJES_PU where fecha_hora > '{fecha} {hora}' order by fecha_hora desc";

                SqlDataReader dr = ejecutarConsulta(query);

                while (dr.Read())
                {
                    Bitacora_Errores_Mensajes_Pu bitacora_Errores_Mensajes_Pu = new Bitacora_Errores_Mensajes_Pu();

                    bitacora_Errores_Mensajes_Pu.consecutivo = dr.GetInt32(0);
                    bitacora_Errores_Mensajes_Pu.fecha_hora = dr.GetDateTime(1);
                    bitacora_Errores_Mensajes_Pu.error_numero = dr.GetDecimal(2);
                    bitacora_Errores_Mensajes_Pu.error_descripcion = dr.GetString(3);
                    bitacora_Errores_Mensajes_Pu.aplicacion = dr.GetString(4);

                    list.Add(bitacora_Errores_Mensajes_Pu);
                }
            }
            catch (SqlException ex)
            {
                Bitacora_Errores_Mensajes_Pu bitacora_Errores_Mensajes_Pu = new Bitacora_Errores_Mensajes_Pu();

                bitacora_Errores_Mensajes_Pu.consecutivo = -1;
                bitacora_Errores_Mensajes_Pu.error_numero = ex.Number;
                bitacora_Errores_Mensajes_Pu.error_descripcion = ex.Message;
                bitacora_Errores_Mensajes_Pu.fecha_hora = DateTime.Now;
                bitacora_Errores_Mensajes_Pu.aplicacion = "Exeption";

                list.Add(bitacora_Errores_Mensajes_Pu);

            }

            return list;

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
        public void Escribe(string vData, string tipo)
        {
            string seccion = "escribeArchivoLOG";

            if (true)
            {
                string fecha = DateTime.Now.ToString("ddMMyyyy");
                string nombre_archivo = $"{fecha}_{getValueAppConfig(seccion, "logFileName")}";

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(getValueAppConfig(seccion, "logFilePath"), nombre_archivo), append: true))
                {
                    vData = $"[{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")}]  {tipo}:  {vData}";
                    Console.WriteLine(vData);
                    outputFile.WriteLine(vData);
                }

            }
        }

        /// <summary>
        /// escribe en el log
        /// </summary>
        /// <param name="vData"></param>
        public void Escribe(Exception ex, string tipo)
        {
            string vData;
            string seccion = "escribeArchivoLOG";

            if (true)
            {
                string fecha = DateTime.Now.ToString("ddMMyyyy");
                string nombre_archivo = $"{fecha}_{getValueAppConfig(seccion, "logFileName")}";

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(getValueAppConfig(seccion, "logFilePath"), nombre_archivo), append: true))
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

        /// <summary>
        /// Ejecutar un insert con un query dado
        /// </summary>
        /// <param name="query">query select</param>
        /// <returns></returns>
        public int ejecutarInsert(string query)
        {
            try
            {
                cnnConexion.ActiveConnection = true;
                cnnConexion.ParametersContains = false;
                cnnConexion.CommandType = CommandType.Text;
                cnnConexion.ActiveConnection = true;

                int afectados = cnnConexion.ExecuteNonQuery(query);

                return afectados;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Inserta fila en BITACORA_ERRORES_MENSAJES_PU
        /// </summary>
        /// <param name="bitacora"></param>
        /// <returns></returns>
        public string psInsertarSQL(Bitacora_Errores_Mensajes_Pu bitacora)
        {
            strQuery = "Insert into BITACORA_ERRORES_MENSAJES_PU ";
            strQuery += "(fecha_hora, error_numero, error_descripcion, aplicacion) ";
            strQuery += $"Values ('{bitacora.fecha_hora}', {bitacora.error_numero}, '{bitacora.error_descripcion}', '{bitacora.aplicacion}')";

            return $"Se afectaron {ejecutarInsert(strQuery)} fila(s)";
        }


        //**************************************************************************************************************

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

    }
}
