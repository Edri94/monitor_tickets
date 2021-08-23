using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mensajes
{
    public class Mensaje
    {
        private string Archivo;
        private string ArchivoIni;
        private string Ls_Archivo;
        private string lsCommandLine;

        // Variables para el control del log
        private string strlogFileName;
        private string strlogFilePath;
        private bool Mb_GrabaLog;

        // Variables para el registro de los valores del header IH
        private string strFuncionHost; // Valor que indica el programa que invocara el CICSBRIDGE
        private string strHeaderTagIni; // Bandera que indica el comienzo del Header
        private string strIDProtocol; // Identificador  del protocolo (PS9)
        private string strLogical; // Terminal Lógico Asigna Arquitectura ASTA
        private string strAccount; // Terminal Contable (CR Contable)
        private string strUser; // Usuario. Debe ser diferente de espacios
        private string strSeqNumber; // Número de Secuencia (indicador de paginación)
        private string strTXCode; // Función específica Asigna Arquitectura Central
        private string strUserOption; // Tecla de función (no aplica)
        private string strCommit; // Indicador de commit: Permite realizar commit
        private string strMsgType; // Tipo de mensaje: Nuevo requerimiento
        private string strProcessType; // Tipo de proceso: on line
        private string strChannel; // Canal Asigna Arquitectura Central
        private string strPreFormat; // Indicador de preformateo: Arquitectura no deberá de preformatear los datos
        private string strLenguage; // Idioma: Español
        private string strHeaderTagEnd; // Bandera que indica el final del header

        // Variables para el registro de los valores del header ME
        private string strMETAGINI; // Bandera que indica el comienzo del mensaje
                                    // Private strMsgColecMax          As String 'Longitud del layout  del colector
        private string strMsgTypeCole; // Tipo de mensaje: Copy
                                       // Private strMaxMsgCole           As String 'Máximo X(30641)
        private string strMETAGEND; // Bandera que indica el fin del mensaje

        // Variables para el registro de los valores Default
        private string strFechaBaja; // fecha_baja
        private string strColectorMaxLeng; // Maxima longitud del COLECTOR
        private string strMsgMaxLeng; // Maxima longitud del del bloque ME
        private string strPS9MaxLeng; // Maxima longitud del formato PS9
        private string strReplyToMQ; // MQueue de respuesta para HOST
        private string strFuncionSQL; // Funcion a ejecutar al recibir la respuesta
        private string strRndLogTerm; // Indica que el atributo Logical Terminal es random

        // Variables para el manejo de los parametros de la base de datos
        // Public gsSeccRegWdw             As String

        // VARIABLES NUVAS PARA EL ENVIO DE MENSAJE
        private string sPersistencia;
        private string sExpirar;

        private string Gs_MQManager;       // MQManager de Escritura
        private string Gs_MQQueueEscritura;       // MQQueue de Escritura
        private string gsEjecutable;       // Ejecutable a realizar

        public string Bandera;

        //MqSeriesConfig mqSeriesConfig;
        //EscribeArchivoLOGConfig escribeArchivoLOGConfig;
        //ConexionConfig conexionConfig;

        MqSeries mQ;

        public Mensaje()
        {
            //mqSeriesConfig = new MqSeriesConfig();
            //escribeArchivoLOGConfig = new EscribeArchivoLOGConfig();
            //conexionConfig = new ConexionConfig();

            mQ = new MqSeries();
        }

        public void ProcesarMensajes(string strRutaIni, string strParametros = "")
        {
            string[] Parametros;       // Arreglo para almacenar los parametros via línea de comando
            string Ls_MsgVal = "";       // Mensaje con el resultado de la validación
            float LnDiferencia;       // Minutos transcurridos desde el último intento de acceso

            //ArchivoIni = strRutaIni + @"\MensajesMQ.ini";
            //gstrRutaIni = ArchivoIni;

            lsCommandLine = strParametros.Trim();

            if (lsCommandLine.Equals("") == false)
            {
                //Array.Clear(Parametros, 0, Parametros.Length);
                Parametros = lsCommandLine.Split('-');
                Gs_MQManager = Parametros[0].Trim();
                Gs_MQQueueEscritura = Parametros[1].Trim();
                gsEjecutable = Parametros[2].Trim();
            }
            else
            {
                ObtenerInfoMQ();
            }

            ConfiguraFileLog();
            ConfiguraHeader_IH_ME();

            if (!ConectDB())
            {
                return;
            }

            Escribe("Comienza la función MAIN de la aplicación MensajesMQ: " + DateTime.Now.ToString("dd/MM/yyyy") + " Tipo Función: '" + gsEjecutable + "'", "Mensaje");
            mQ.gsAccesoActual = DateTime.Now.ToString();

            if (!ValidaInfoMQ(Ls_MsgVal))
            {
               
                mQ.psInsertarSQL(
                    new Bitacora_Errores_Mensajes_Pu
                    {
                        fecha_hora = DateTime.Parse(mQ.gsAccesoActual),
                        error_numero = 1,
                        error_descripcion = Ls_MsgVal,
                        aplicacion = "MSG"
                    }
                );

                Escribe("Termina el acceso a la aplicación MensajesMQ. Cheque la bitácora de errores en SQL. Tipo Función: '" + gsEjecutable + "'", "Mensaje");
                Desconectar();
                return;
            }

            switch (gsEjecutable)
            {
                case "F":
                    ProcesoBDtoMQQUEUEFunc();
                    break;
                case "A":
                    ProcesoBDtoMQQUEUEAuto();
                    break;
                default:
                    break;
            }

            Escribe("Termina el acceso a la aplicación MensajesMQ. Función SQL: " + strFuncionSQL, "Mensaje");
        }

        private void ObtenerInfoMQ()
        {
            string section = "mqSeries";
            Gs_MQManager = getValueAppConfig(section,"MQManager");
            Gs_MQQueueEscritura = getValueAppConfig(section, "MQEscritura");
            gsEjecutable = getValueAppConfig(section, "FGEjecutable");
        }

        private void ConfiguraFileLog()
        {
            string section = "escribeArchivoLOG";

            strlogFileName =  getValueAppConfig(section, "logFileName");
            strlogFilePath =  getValueAppConfig(section, "logFilePath");
            string estatus_str =  getValueAppConfig(section, "Estatus");
            Mb_GrabaLog = (Int32.Parse(estatus_str) == 1)? true : false;
        }

        private bool ConectDB()
        {
            bool ConectDB = false;
            string section = "conexion";
            try
            {
                mQ.gsCataDB = getValueAppConfig(section, "DBCata");
                mQ.gsDSNDB = getValueAppConfig(section, "DBDSN");
                mQ.gsSrvr = getValueAppConfig(section, "DBSrvr");
                mQ.gsUserDB = getValueAppConfig(section, "DBUser");
                mQ.gsPswdDB = getValueAppConfig(section, "DBPswd");
                mQ.gsNameDB = getValueAppConfig(section, "DBName");

                string conn_str = $"Data source ={mQ.gsSrvr}; uid ={mQ.gsUserDB}; PWD ={mQ.gsPswdDB}; initial catalog = {mQ.gsNameDB}";
                mQ.cnnConexion = new ConexionBDSQL.ConexionBD(conn_str);

                //List<Bitacora_Errores_Mensajes_Pu> lst = mQ.ConsultaBitacoraErroresMensajes("2021-01-01", "00:00:00");

                ConectDB = true;

                return ConectDB;

            }
            catch (Exception ex)
            {
                Escribe(ex, "Error");
            }
            return ConectDB;
        }

        private void Desconectar()
        {
            //mQ.cnnConexion.Close();
            //Set rssRegistro = Nothing
            mQ.cnnConexion = null;
        }

        private void ConfiguraHeader_IH_ME()
        {
            string section = "headerih";

            strFuncionHost =  getValueAppConfig(section,"PRIMERVALOR");
            strHeaderTagIni =  getValueAppConfig(section,"IHTAGINI");
            strIDProtocol =  getValueAppConfig(section,"IDPROTOCOL");
            strLogical =  getValueAppConfig(section,"Logical");
            strAccount =  getValueAppConfig(section,"ACCOUNT");
            strUser =  getValueAppConfig(section,"User");
            strSeqNumber =  getValueAppConfig(section,"SEQNUMBER");
            strTXCode =  getValueAppConfig(section,"TXCODE");
            strUserOption =  getValueAppConfig(section,"USEROPT");
            strCommit =  getValueAppConfig(section,"Commit");
            strMsgType =  getValueAppConfig(section,"MSGTYPE");
            strProcessType =  getValueAppConfig(section,"PROCESSTYPE");
            strChannel =  getValueAppConfig(section,"CHANNEL");
            strPreFormat =  getValueAppConfig(section,"PREFORMATIND");
            strLenguage =  getValueAppConfig(section,"LANGUAGE");
            strHeaderTagEnd =  getValueAppConfig(section,"IHTAGEND");

            section = "headerme";

            strMETAGINI = getValueAppConfig(section, "METAGINI");
            strMsgTypeCole = getValueAppConfig(section, "TIPOMSG");
            strMETAGEND = getValueAppConfig(section, "METAGEND");

            section = "defaultValues";

            strFechaBaja = getValueAppConfig(section, "FECHABAJA");
            strColectorMaxLeng = getValueAppConfig(section, "COLMAXLENG");
            strMsgMaxLeng = getValueAppConfig(section, "MSGMAXLENG");
            strPS9MaxLeng = getValueAppConfig(section, "PS9MAXLENG");
            strReplyToMQ = getValueAppConfig(section, "ReplyToQueue");

            switch (gsEjecutable)
            {
                case "F":
                    strFuncionSQL = getValueAppConfig(section, "FuncionSQLF");
                    break;
                case "A":
                    strFuncionSQL = getValueAppConfig(section, "FuncionSQLA");
                    break;
            }
            strRndLogTerm =getValueAppConfig(section, "RandomLogTerm");
            sPersistencia =getValueAppConfig(section, "PPERSISTENCE");
            sExpirar =getValueAppConfig(section, "PEXPIRY");
        }

        private bool ValidaInfoMQ(string ps_MsgVal)
        {
            string ls_msg = "";

            if (Gs_MQManager.Trim() == "")
            {
                ls_msg = ls_msg + "";
            }
            if (Gs_MQQueueEscritura.Trim() == "")
            {
                ls_msg = ls_msg + "";
            }
            if (ls_msg == "")
            {
                return true;
            }
            ps_MsgVal = ls_msg;
            return false;
        }

        private void ProcesoBDtoMQQUEUEFunc()
        {
            string Ls_MensajeMQ;       // Cadena con el mensaje armado con los registros de la base de datos
            string Ls_MsgColector;       // Cadena para almecenar el COLECTOR
            string Ls_HeaderMsg;       // Cadena para almacenar el HEADER del mensaje
            int NumeroMsgEnviados;      // Contador para almacenar el número de mensajes procesados
            string[] las_Funcionarios = null;       // Arreglo para ingresar todos los registros que han sido enviados correctamente
                                             // Para el armado de la solicitud
            string ls_IDFuncionario;
            string ls_CentroRegional;       // 1  centro_regional
            string ls_NumRegistro;       // 2  numero_registro
            string ls_Producto;       // 3  producto
            string ls_SubProducto;       // 4  subproducto
            string ls_FechaAlta = "0000/00/00";       // 5  fecha_alta
            string ls_TipoPeticion;       // 8  tipo_peticion
            string ls_IdTransaccion;       // 12 id_transaccion
            string ls_Tipo;       // 13 tipo
            string ls_Fecha;
            string ls_Hora = "00:00";

            string strQuery;

            try
            {
                Escribe("Inicia el envío de mensajes a Host: " + mQ.gsAccesoActual + " Función: " + strFuncionSQL, "Mensaje");
                NumeroMsgEnviados = 0;

                // Logica para recuperar los n mensajes de la tabla temporal en db.funcionario
                // Logica para procesar cada registro y convertirlo en un mensaje
                strQuery = "SELECT" + (char)13;
                strQuery = strQuery + "id_funcionario," + (char)13;                       // 0  id_funcionario
                strQuery = strQuery + "centro_regional," + (char)13;                      // 1  centro_regional
                strQuery = strQuery + "numero_funcionario," + (char)13;                   // 2  numero_
                strQuery = strQuery + "producto," + (char)13;                             // 3  producto
                strQuery = strQuery + "subproducto," + (char)13;                          // 4  subproducto
                strQuery = strQuery + "CONVERT(char(11), fecha_alta, 105) + CONVERT(char(5), fecha_alta, 108) [fecha_alta]," + (char)13;                           // 5  fecha_alta
                strQuery = strQuery + "CONVERT(char(11), fecha_baja, 105) + CONVERT(char(5), fecha_baja, 108) [fecha_baja], " + (char)13;                           // 6  fecha_baja
                strQuery = strQuery + "CONVERT(char(11), fecha_ultimo_mant, 105) + CONVERT(char(6), fecha_ultimo_mant, 108) [fecha_ultimo_mant]," + (char)13;                    // 7  fecha_ultimo_mant
                strQuery = strQuery + "tipo_peticion [tipo_peticion]," + (char)13;                        // 8  tipo_peticion
                strQuery = strQuery + "status_envio [status_envio]," + (char)13;                          // 9  status_envio
                strQuery = strQuery + "CONVERT(char(8),getdate(),112) [columna_11]," + (char)13;        // 10
                strQuery = strQuery + "CONVERT(char(5),getdate(),108) [columna_12]," + (char)13;        // 11
                strQuery = strQuery + "id_transaccion," + (char)13;                        // 12  id transaccion en TKT
                strQuery = strQuery + "tipo " + (char)13;                                  // 13  Tipo  A-Alta, B-Baja, M-Mantenimiento
                strQuery = strQuery + "FROM" + (char)13;
                strQuery = strQuery + mQ.gsNameDB + "..TMP_FUNCIONARIOS_PU" + (char)13;
                strQuery = strQuery + "WHERE status_envio = 0";

                DataTable rssRegistro = mQ.ConsultaMQQUEUEFunc(strQuery);

                if (rssRegistro != null) //Not rssRegistro.EOF
                {
                    if (mQ.MQConectar(Gs_MQManager, mQ.mqManager))
                    {
                        mQ.blnConectado = true;
                    }
                    else
                    {
                        
                        mQ.psInsertarSQL(
                            new Bitacora_Errores_Mensajes_Pu
                            {
                                fecha_hora = DateTime.Parse(mQ.gsAccesoActual),
                                error_numero = 3,
                                error_descripcion = "ProcesoBDtoMQQUEUEFunc. Fallo conexión MQ-Manager " + Gs_MQManager,
                                aplicacion = "MSG"
                            }
                        );

                        return;
                    }

                    foreach (DataRow row in rssRegistro.Rows)
                    {
                        //Almacenando variables
                        ls_IDFuncionario = Left(row[""].ToString(), 7);
                        ls_CentroRegional = Left(row[""].ToString(), 4);
                        ls_NumRegistro = Left(row[""].ToString(), 8);
                        ls_Producto = Left(row[""].ToString(), 2);
                        ls_SubProducto = Left(row[""].ToString(), 10);

                        if (row[""].ToString() != "")
                        {
                            ls_FechaAlta = row[""].ToString();
                            //ls_FechaAlta = Mid(ls_FechaAlta, 1, 10)
                        }

                        ls_TipoPeticion = Left(row[""].ToString(), 1);
                        ls_Fecha = Left(row[""].ToString() + Space(8), 8);
                        ls_IdTransaccion = Left(row[""].ToString(), 10);
                        ls_Tipo = Left(row[""].ToString(), 1);

                        Ls_MsgColector = Left(strFuncionSQL.Trim() + "        ", 8);
                        Ls_MsgColector = Ls_MsgColector + ls_Fecha + ls_Hora;
                        Ls_MsgColector = Ls_MsgColector + ls_TipoPeticion + ls_CentroRegional;
                        Ls_MsgColector = Ls_MsgColector + ls_NumRegistro + ls_Producto;
                        Ls_MsgColector = Ls_MsgColector + ls_SubProducto + ls_FechaAlta;
                        Ls_MsgColector = Ls_MsgColector + strFechaBaja + ls_IDFuncionario;
                        Ls_MsgColector = Ls_MsgColector + ls_IdTransaccion + ls_Tipo;
                        Ls_MsgColector = Ls_MsgColector + Space(43);

                        if (Ls_MsgColector.Length > 0)
                        {
                            Ls_MensajeMQ = ASTA_ENTRADA(Ls_MsgColector, " Funcionario: " + ls_IDFuncionario);

                            if (Ls_MensajeMQ != "")
                            {
                                Escribe("Mensaje Enviado: " + Ls_MensajeMQ, "Mensaje");
                                if (mQ.MQEnviarMsg(mQ.mqManager, Gs_MQQueueEscritura, mQ.mqsEscribir, mQ.mqsMsgEscribir, Ls_MensajeMQ, strReplyToMQ, sPersistencia, sExpirar))
                                {
                                    //ReDim Preserve las_Funcionarios(NumeroMsgEnviados)
                                    las_Funcionarios[NumeroMsgEnviados] = ls_IDFuncionario;
                                    NumeroMsgEnviados = NumeroMsgEnviados + 1;
                                }
                                else
                                {
                                    mQ.psInsertarSQL(
                                       new Bitacora_Errores_Mensajes_Pu
                                       {
                                           fecha_hora = DateTime.Parse(mQ.gsAccesoActual),
                                           error_numero = 4,
                                           error_descripcion = "ProcesoBDtoMQQUEUEFunc. Error al escribir la solicitud en la MQ QUEUE: " + Gs_MQQueueEscritura + ". Error con el Funcionario: " + ls_IDFuncionario,
                                           aplicacion = "MSG"
                                       }
                                   );
                                }
                            }
                            else
                            {
                                mQ.psInsertarSQL(
                                       new Bitacora_Errores_Mensajes_Pu
                                       {
                                           fecha_hora = DateTime.Parse(mQ.gsAccesoActual),
                                           error_numero = 4,
                                           error_descripcion = "ProcesoBDtoMQQUEUEFunc. Error durante el armado del formato PS9 funcion ASTA_ENTRADA. Error con el Funcionario: " + ls_IDFuncionario,
                                           aplicacion = "MSG"
                                       }
                                   );
                            }
                        }
                        else
                        {
                            Escribe("Error al armar el Layout Alta-Mantenimiento-Baja de Funcionarios TKT-CED. Error con el Funcionario : " + ls_IDFuncionario, "Mensaje");
                        }
                    }                              
                }
                else
                {
                    Escribe("No existen registros en la consulta de los datos de tabla TMP_FUNCIONARIOS_PU. ProcesoBDtoMQQUEUEFunc", "Mensaje");
                }
                mQ.MQDesconectar(mQ.mqManager, mQ.mqsEscribir);

                if (NumeroMsgEnviados > 0)
                {
                    if (!ActualizaRegistrosFunc(las_Funcionarios))
                    {
                        Escribe("Existieron errores al actualizar la tabla TMP_FUNCIONARIOS_PU", "Mensaje");
                    }
                }

                Escribe("Envio de solicitures TKT -> Host Terminado. ProcesoBDtoMQQUEUEFunc", "Mensaje");
                Escribe("Solicitudes enviadas a MQ: " + NumeroMsgEnviados, "Mensaje");
            }
            catch (Exception Err)
            {
                Escribe("Se presentó un error durante la ejecución de la función ProcesoBDtoMQQUEUEFunc. Vea log y tabla TMP_FUNCIONARIOS_PU. ", "Error");
                Escribe(Err, "Error");
            }



        }

        private void ProcesoBDtoMQQUEUEAuto()
        {
            string Ls_MensajeMQ;       // Cadena con el mensaje armado con los registros de la base de datos
            string Ls_MsgColector;       // Cadena para almecenar el COLECTOR
            string Ls_HeaderMsg;       // Cadena para almacenar el HEADER del mensaje
            string strQuery;       // Cadena para almacenar el Query a ejecutarse en la base de datos
            int NumeroMsgEnviados;      // Contador para almacenar el número de mensajes procesados
            string[] las_Autorizaciones;    // Arreglo para ingresar todos los registros que han sido enviados correctamente
                                            // Para el armado de la solicitud
            string ls_Operacion;    // 1  operacion
            string ls_Oficina;    // 2  oficina
            string ls_NumeroFunc;    // 3  codusu
            string ls_Transaccion;    // 4  transaccion
            string ls_CodigoOperacion;    // 5  tipo-oper
            string ls_Cuenta;    // 6  cuenta-ced
            string ls_Divisa;    // 7  divisa
            string ls_Importe;    // 8  importe
            string ls_Fecha_Ope;    // 9  Fecha (operacion)
            string ls_Folio_Ope;    // 10 Folio
            string ls_Status_Envio;    // 11 Status
            string ls_Fecha;
            string ls_Hora;

            strQuery = "";


            try
            {             
                Escribe("Inicia el envío de mensajes a Host: " + mQ.gsAccesoActual + " Función: " + strFuncionSQL, "Mensaje");
                NumeroMsgEnviados = 0;

                strQuery = "SELECT" + (char)13;
                strQuery = strQuery + "operacion," + (char)13;
                strQuery = strQuery + "oficina," + (char)13;
                strQuery = strQuery + "numero_funcionario," + (char)13;
                strQuery = strQuery + "id_transaccion," + (char)13;
                strQuery = strQuery + "codigo_operacion," + (char)13;
                strQuery = strQuery + "cuenta," + (char)13;
                strQuery = strQuery + "divisa," + (char)13;
                strQuery = strQuery + "importe," + (char)13;
                strQuery = strQuery + "fecha_operacion," + (char)13;
                strQuery = strQuery + "folio_autorizacion," + (char)13;
                strQuery = strQuery + "status_envio," + (char)13;
                strQuery = strQuery + "CONVERT(char(8),getdate(),112) [fecha]," + (char)13;
                strQuery = strQuery + "CONVERT(char(5),getdate(),108) [hora]" + (char)13;
                strQuery = strQuery + "FROM " + (char)13;
                strQuery = strQuery + "TMP_AUTORIZACIONES_PU" + (char)13;
                //strQuery = strQuery + "WHERE status_envio = 0";
                strQuery = strQuery + "WHERE status_envio = 1 AND CONVERT(DATETIME, fecha_operacion, 12) > '2020-01-01 00:00:00'";

                DataTable rssRegistro = mQ.ConsultaMQQUEUEAuto(strQuery);
                las_Autorizaciones = null;

                if (rssRegistro.Rows.Count > 0)
                {
                    if (mQ.MQConectar(Gs_MQManager, mQ.mqManager))
                    {
                        mQ.blnConectado = true;
                    }
                    else
                    {
                        mQ.psInsertarSQL(
                            new Bitacora_Errores_Mensajes_Pu
                            {
                                fecha_hora = DateTime.Parse(mQ.gsAccesoActual),
                                error_numero = 3,
                                error_descripcion = "ProcesoBDtoMQQUEUEAuto. Fallo conexión MQ-Manager " + Gs_MQManager,
                                aplicacion = "MSG"
                            }
                        );
                        return;
                    }

                    int i = 0;

                    foreach(DataRow row in rssRegistro.Rows)
                    {
                        i++;

                        ls_Operacion = Int32.Parse(row["operacion"].ToString()).ToString("D7");
                        ls_Oficina = Int32.Parse(mQ.rssRegistro[1].Trim()).ToString("D4");
                        ls_NumeroFunc = mQ.rssRegistro[2].Trim() + Space(8 - mQ.rssRegistro[2].Trim().Length);
                        ls_Transaccion = Int32.Parse(mQ.rssRegistro[3].Trim()).ToString("D4");
                        ls_CodigoOperacion = mQ.rssRegistro[4].Trim() + Space(3);
                        ls_Cuenta = mQ.rssRegistro[5].Trim() + Space(10);
                        ls_Divisa = mQ.rssRegistro[6].Trim() + Space(3);
                        ls_Importe = mQ.rssRegistro[7].Trim();
                        ls_Fecha_Ope = mQ.rssRegistro[8].Trim();
                        ls_Folio_Ope = Int32.Parse(mQ.rssRegistro[9].Trim()).ToString("D12");
                        ls_Status_Envio = Int32.Parse(mQ.rssRegistro[10].Trim()).ToString("D1");

                        ls_Fecha = Left(mQ.rssRegistro[0].Trim() + Space(8), 8);
                        ls_Hora = Left(mQ.rssRegistro[0].Trim().Replace(':', ' ') + Space(4), 4);

                        Ls_MsgColector = Left(strFuncionSQL.Trim() + "        ", 8);
                        Ls_MsgColector = Ls_MsgColector + ls_Fecha + ls_Hora;
                        Ls_MsgColector = Ls_MsgColector + ls_Operacion + ls_Oficina;
                        Ls_MsgColector = Ls_MsgColector + ls_NumeroFunc + ls_Transaccion;
                        Ls_MsgColector = Ls_MsgColector + ls_CodigoOperacion + ls_Cuenta;
                        Ls_MsgColector = Ls_MsgColector + ls_Divisa + ls_Importe;
                        Ls_MsgColector = Ls_MsgColector + ls_Fecha_Ope + ls_Folio_Ope;

                        if (Ls_MsgColector.Length > 0)
                        {
                            Ls_MensajeMQ = ASTA_ENTRADA(Ls_MsgColector, " Operación: " + ls_Operacion);
                            if (Ls_MensajeMQ != "")
                            {
                                Escribe("Mensaje Enviado: " + Ls_MensajeMQ, "Mensaje");
                                if (mQ.MQEnviarMsg(mQ.mqManager, Gs_MQQueueEscritura, mQ.mqsEscribir, mQ.mqsMsgEscribir, Ls_MensajeMQ, strReplyToMQ, sPersistencia, sExpirar))
                                {
                                    las_Autorizaciones[NumeroMsgEnviados] = ls_Operacion;
                                    NumeroMsgEnviados = NumeroMsgEnviados + 1;
                                }
                                else
                                {
                                    mQ.psInsertarSQL(
                                        new Bitacora_Errores_Mensajes_Pu
                                        {
                                            fecha_hora = DateTime.Parse(mQ.gsAccesoActual),
                                            error_numero = 5,
                                            error_descripcion = "ProcesoBDtoMQQUEUEAuto. Error al escribir la solicitud en la MQ QUEUE: " + Gs_MQQueueEscritura + ". Error con la Operación: " + ls_Operacion,
                                            aplicacion = "MSG"
                                        }
                                    );
                                }
                            }
                            else
                            {
                                mQ.psInsertarSQL(
                                    new Bitacora_Errores_Mensajes_Pu
                                    {
                                        fecha_hora = DateTime.Parse(mQ.gsAccesoActual),
                                        error_numero = 4,
                                        error_descripcion = "ProcesoBDtoMQQUEUEAuto. Error durante el armado del formato PS9 funcion ASTA_ENTRADA. Error con la Operacion: " + ls_Operacion,
                                        aplicacion = "MSG"
                                    }
                                );
                            }
                        }
                        else
                        {
                            Escribe("Error al armar el Layout Actualización del Autorizaciones TKT-CED. Error con la Operación : " + ls_Operacion, "Mensaje");
                        }
                    }

                    //do
                    //{
                        

                    //} while (i < mQ.rssRegistro.Count());
                }
                else
                {
                    Escribe("Cero registros en la consulta de los datos, tabla TMP_AUTORIZACIONES_PU. ProcesoBDtoMQQUEUEAuto", "Mensaje");
                }

                mQ.MQDesconectar(mQ.mqManager, mQ.mqsEscribir);

                if (NumeroMsgEnviados > 0)
                {
                    if (!ActualizaRegistrosAuto(las_Autorizaciones))
                    {
                        Escribe("Existieron errores al actualizar la tabla TMP_AUTORIZACIONES_PU", "Mensaje");
                    }
                }
            }
            catch (Exception Err)
            {
                Escribe("Se presentó un error durante la ejecución de la función ProcesoBDtoMQQUEUEAuto. Vea log y tabla TMP_AUTORIZACIONES_PU. ", "Error");
                Escribe(Err, "Error");
            }

        }

           private string ASTA_ENTRADA(string strMsgColector, string psTipo)
        {
            string ls_TempColectorMsg;
            string ls_BloqueME;
            int ln_longCOLECTOR;
            int ln_AccTerminal;

            string ASTA_ENTRADA = "";

            try
            {
                ls_TempColectorMsg = strMsgColector;

                if(ls_TempColectorMsg.Length >  Int32.Parse(strColectorMaxLeng))
                {
                    Escribe("La longitud del colector supera el maximo permitido","Mensaje");
                    return "ErrorASTA";
                }

                ls_BloqueME = Left(strMETAGINI +"    ", 4);
                ls_BloqueME = ls_BloqueME + Right("0000" + ls_TempColectorMsg.Length.ToString(),4);
                ls_BloqueME = ls_BloqueME + Left(strMsgTypeCole.Trim() + " ", 1);
                ls_BloqueME = ls_BloqueME + ls_TempColectorMsg;
                ls_BloqueME = ls_BloqueME + Left(strMETAGEND.Trim() + "     ", 5);


                if(ls_BloqueME.Length > Int32.Parse(strMsgMaxLeng.Trim()))
                {
                    Escribe("La longitud del Bloque ME supera el maximo permitido", "Mensaje");
                    return "ErrorASTA";
                }

                //'Para el uso de MQ-SERIES y CICSBRIDGE se requiere anteponer
                //'al HEADER DE ENTRADA(IH) un valor que indique el programa
                //'que invocara el CICSBRIDGE
                //'X(08)  Indica el programa que invocara el CICSBRIDGE
                ASTA_ENTRADA = Left(strFuncionHost.Trim() + "        ", 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strHeaderTagIni.Trim() + "    ", 4);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strIDProtocol.Trim() + "  ", 2);

                if(strRndLogTerm.Trim().Equals("1"))
                {
                    ln_AccTerminal = 0;
                    do
                    {
                        var Rnd = new Random(DateTime.Now.Second * 1000);
                        ln_AccTerminal = Rnd.Next();
                    } while (ln_AccTerminal > 0 && ln_AccTerminal < 2000);
                    ASTA_ENTRADA = ASTA_ENTRADA + Left(ln_AccTerminal.ToString("D4") + "        ", 8); ;
                }
                else
                {
                    ASTA_ENTRADA = ASTA_ENTRADA + Left(strLogical.Trim() + "        ", 8);
                }


                ASTA_ENTRADA = ASTA_ENTRADA + Left(strAccount.Trim() + "        ", 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strUser.Trim() + "        ", 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strSeqNumber.Trim() + "        ", 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strTXCode.Trim() + "        ", 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strUserOption.Trim() + "  ", 2);

                ln_longCOLECTOR = 65 + ls_BloqueME.Length;

                if(ln_longCOLECTOR > Int32.Parse(strPS9MaxLeng))
                {
                    Escribe("La longitud del Layout PS9 supera el maximo permitido", "Mensaje");
                    return "ErrorASTA";
                }

                ASTA_ENTRADA = ASTA_ENTRADA + Right("00000" + ln_longCOLECTOR.ToString(), 5);
                ASTA_ENTRADA = ASTA_ENTRADA + Left((strCommit).Trim() + " ", 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left((strMsgType).Trim() + " ", 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left((strProcessType).Trim() + " ", 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left((strChannel).Trim() + "  ", 2);
                ASTA_ENTRADA = ASTA_ENTRADA + Left((strPreFormat).Trim() + " ", 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left((strLenguage).Trim() + " ", 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left((strHeaderTagEnd).Trim() + "     ", 5);
                ASTA_ENTRADA = ASTA_ENTRADA + ls_BloqueME;


            }
            catch (Exception Err)
            {
                Escribe("Error al armar el mensaje para " + psTipo, "Error" );
                Escribe(Err, "Error");
            }

            return "";
        }

        private bool ActualizaRegistrosFunc(string[] IDFuncionario)
        {
            bool ActualizaRegistrosFunc = false;

            string strQueryUpDate;
            int ln_indice;

            ActualizaRegistrosFunc = true;

            try
            {
                for (ln_indice = 0; ln_indice < (IDFuncionario.Count()); ln_indice++)
                {
                    strQueryUpDate = "UPDATE TMP_FUNCIONARIOS_PU" + (char)13;
                    strQueryUpDate = strQueryUpDate + "SET  status_envio = 1" + (char)13;
                    strQueryUpDate = strQueryUpDate + "--  ,fecha_ultimo_mant = GETDATE()," + (char)13;
                    strQueryUpDate = strQueryUpDate + "WHERE status_envio = 0" + (char)13;
                    strQueryUpDate = strQueryUpDate + "AND id_funcionario = " + IDFuncionario[ln_indice];

                    //rssRegistro.Open strQueryUpDate
                }
            }
            catch (Exception Err)
            {
                Escribe("Error al realizar la actualización en la tabla TMP_FUNCIONARIOS_PU. Función ActualizaRegistrosFunc. ", "Error");
                Escribe(Err, "Error");
            }

            return ActualizaRegistrosFunc;
        }

        private bool ActualizaRegistrosAuto(string[] IDAutorizacion)
        {
            bool ActualizaRegistrosAuto = false;

            try
            {
                string strQueryUpDate;
                int ln_indice;

                for (ln_indice = 0; ln_indice < IDAutorizacion.Count(); ln_indice++)
                {
                    strQueryUpDate = "UPDATE " + mQ.gsNameDB + "..TMP_AUTORIZACIONES_PU " + (char)13;
                    strQueryUpDate = strQueryUpDate + "SET  status_envio = 1 " + (char)13;
                    strQueryUpDate = strQueryUpDate + "WHERE status_envio = 0 " + (char)13;
                    strQueryUpDate = strQueryUpDate + "AND operacion = " + IDAutorizacion[ln_indice];
                    //rssRegistro.Open strQueryUpDate
                }
                ActualizaRegistrosAuto = true;
                return ActualizaRegistrosAuto;

            }
            catch (Exception Err)
            {
                Escribe("Error al realizar la actualización en la tabla TMP_AUTORIZACION_PU. Función ActualizaRegistrosAuto. ", "Error");
                Escribe(Err, "Error");
            }

            return ActualizaRegistrosAuto;
        }

        //**********************************FUNCIONES EXTRAS********************************** 

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
        /// Devuelve cadena igual a el numero de espacios dados en el parametro
        /// </summary>
        /// <param name="veces">numero de espacios</param>
        /// <returns></returns>
        public string Space(int veces)
        {
            return new String(' ', veces);
        }


        /// <summary>
        /// Devuelve una variante ( cadena ) que contiene un número especificado de caracteres del lado izquierdo de una cadena.
        /// </summary>
        /// <param name="cadena">Cadena</param>
        /// <param name="posiciones">Posiciones a tomar</param>
        /// <returns></returns>
        public string Left(string cadena, int posiciones)
        {
            return cadena.Substring(0, posiciones);
        }
        /// <summary>
        /// Devuelve una variante ( cadena ) que contiene un número específico de caracteres del lado derecho de una cadena.
        /// </summary>
        /// <param name="cadena">Cadena</param>
        /// <param name="posiciones">Posiciones a tomar</param>
        /// <returns></returns>
        public string Right(string cadena, int posiciones)
        {
            return cadena.Substring((cadena.Length - posiciones), posiciones);
        }

        /// <summary>
        /// Escribe en el App.config en la seccion y key dada en parametros
        /// </summary>
        /// <param name="section">seccion en appsettings</param>
        /// <param name="key">key en appsetitngs</param>
        /// <param name="value">valor nuevo</param>
        public void SetParameterAppSettings(string section, string key, string value)
        {
            try
            {
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string[] appPath_arr = appPath.Split('\\');

                Escribe("Variable entrada [appPath]: " + appPath, "Mensaje");
                appPath = "";
                for (int i = 0; i < (appPath_arr.Length - 1); i++)
                {
                    appPath = appPath + "\\" + appPath_arr[i];
                }
                appPath = appPath.Substring(1, appPath.Length - 1);
                Escribe("Variable entrada [appPath]: " + appPath, "Mensaje");
                string configFile = System.IO.Path.Combine(appPath, "App.config");
                Escribe("Variable entrada [configFile]: " + configFile, "Mensaje");
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configFile;
                System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                config.AppSettings.Settings[$"{section}.{key}"].Value = value;

                config.Save();
            }
            catch (Exception ex)
            {
                Escribe(ex, "Error");
            }

        }

        /// <summary>
        /// escribe en el log
        /// </summary>
        /// <param name="vData"></param>
        public void Escribe(string vData, string tipo)
        {
            string seccion = "escribeArchivoLOG";

            if (Mb_GrabaLog)
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
        /// escribe en el log
        /// </summary>
        /// <param name="vData"></param>
        public void Escribe(Exception ex, string tipo)
        {
            string vData;
            string seccion = "escribeArchivoLOG";

            if (Mb_GrabaLog)
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
