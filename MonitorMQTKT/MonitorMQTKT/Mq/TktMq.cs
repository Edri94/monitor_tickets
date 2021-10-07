using IBM.WMQ;
using MonitorMQTKT.Funciones;
using MonitorMQTKT.Models;
using MonitorMQTKT.Processes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorMQTKT.Mq
{
    public class TktMq : MqSeries
    {
        new readonly Funcion funcion;
        public string strReturn;
        public bool mbFuncionBloque;


        public TktMq()
        {
            funcion = new Funcion();
        }
        public double RevisaQueueMq(string strMQCola, MQOPEN lngOpciones)
        {
            //bMQAbrirCola = false;
            double MQRevisaQueue;
            MQRevisaQueue = 0;
            funcion.Escribe("", " ---> entro a MQRevisaQueue");
            try
            {   //' Se accesa la cola ya sea para leer o escribir
                funcion.Escribe(" ---> conectandose a la Queue: " + strMQCola, "Mensaje");
                funcion.Escribe(" ---> objMQManager: " + QMGR.Name, "Mensaje");

                QUEUE = QMGR.AccessQueue(strMQCola, (int)lngOpciones);

                funcion.Escribe("", " ---> se conecto EXITOSAMENTE a la Queue: " + strMQCola);
                funcion.Escribe("", " ---> numero de mensajes en la Queue: " + QUEUE.CurrentDepth.ToString());
                //bMQAbrirCola = true;
                MQRevisaQueue = QUEUE.CurrentDepth;
                return MQRevisaQueue;
            }
            catch (MQException ex)
            {
                //bMQAbrirCola = false;
                MQRevisaQueue = 0;
                funcion.Escribe("", ": Error en el acceso del QManager. funcion: MQRevisaQueue() ; " + ex.ReasonCode + " - " + ex.Reason);
                return MQRevisaQueue;
            }
        }

        public bool RecibirMq(string QueueName)
        {
            //cInterfaz escribeLog = new cInterfaz();
            funcion.Escribe("Entro a funion MQRecibir", "Mensaje");
            try
            {
                funcion.Escribe("Intentando acceder a la queue:" + QueueName, "Mensaje");
                //int openOptions = MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_OUTPUT;
                int openOptions = MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING;
                //queue = mqsManager.AccessQueue(QueueName, (int)lngOpciones);
                QUEUE = QMGR.AccessQueue(QueueName, openOptions);
                funcion.Escribe("Acceso a la queue satisfactorio." + QUEUE.Name, "Mensaje");

                funcion.Escribe("Instanciando el objeto queueMessage como MQMessage", "Mensaje");
                //queueMessage = new MQMessage();
                MQMessage qMessage = new MQMessage();
                //qMessage.MessageId = 

                funcion.Escribe("Objeto instanciado correctamente.", "Mensaje");
                //queueMessage.Format = MQC.MQFMT_STRING;
                //qMessage.Format = MQC.MQFMT_STRING;

                //Se accesan a la opciones de lectura por default
                funcion.Escribe("Instanciando Opciones del Mensaje", "Mensaje");
                MQGetMessageOptions queueGetMessageOptions = new MQGetMessageOptions();
                queueGetMessageOptions.WaitInterval = 2 * 1000;
                queueGetMessageOptions.Options = MQC.MQGMO_WAIT;

                funcion.Escribe("Asignando Opciones al objeto del Mensaje", "Mensaje");
                //queueGetMessageOptions.Options = MQC.MQGMO_NO_WAIT || MQC.MQGMO_COMPLETE_MSG;

                funcion.Escribe("Haciendo GET para obtener Mensaje: ", "Mensaje");
                QUEUE.Get(qMessage, queueGetMessageOptions);

                byte[] byteMessageId = null;
                string strMessageId = "";
                strReturn = "";
                if (qMessage.Format.CompareTo(MQC.MQFMT_STRING) == 0)
                {
                    qMessage.Seek(0);
                    strReturn = System.Text.UTF8Encoding.UTF8.GetString(qMessage.ReadBytes(qMessage.MessageLength));
                    byteMessageId = qMessage.MessageId;
                    strMessageId = qMessage.MessageId.ToString();
                }
                else
                {
                    throw new NotSupportedException(string.Format("Unsupported message format: '{0}' read from queue: {1}.", qMessage.Format, QUEUE));
                }


                //Obtener el Id del mensage para el regreso
                //string strMessageId = queueMessage.MessageId.ToString();
                //string strMessageId = qMessage.MessageId.ToString();
                funcion.Escribe("strMessageId:" + strMessageId, "Mensaje");
                //string strCorrelId = queueMessage.CorrelationId.ToString();
                //string strCorrelId = qMessage.CorrelationId.ToString();
                //Escribe("strCorrelId:" + strCorrelId, "Mensaje");

                //strReturn = queueMessage.ReadString(queueMessage.MessageLength);
                //strReturn = qMessage.ReadUTF();
                funcion.Escribe("strReturn:" + strReturn, "Mensaje");

                string msgRecuperado = "Mensaje recuperado de la queue: " + strReturn;
                funcion.Escribe(msgRecuperado, "Mensaje");
            }
            catch (MQException MQexp)
            {
                string strCadenaLogMQ = "Error al leer Queue " + MQexp.Reason + " " +
                    MQexp.InnerException + " , " + MQexp.TargetSite + " , " + MQexp.Data +
                    +MQexp.ReasonCode + ", mensaje " + MQexp.Source + "  Error " + MQexp.Message;
                QMGR.Close();
                funcion.Escribe(strCadenaLogMQ, "Mensaje");
                return false;
            }
            finally
            {
                QUEUE.Close();
            }
            return true;
        }

        public bool MQEnviar(string strMQCola, string ls_mensaje)
        {
            try
            {
                MQPutMessageOptions mqsMQOpciones = new MQPutMessageOptions();
                string strMensaje;

                if (AbrirColaMQ(strMQCola, MQOPEN.MQOO_OUTPUT))
                {
                    strMensaje = ls_mensaje;

                    MSG.ClearMessage();
                    MSG.Format = "MQSTR   ";
                    //objMQMensaje.MessageType = 0;
                    MSG.MessageType = MQC.MQMT_DATAGRAM;
                    MSG.WriteLong(strMensaje.Trim().Length);
                    MSG.WriteUTF(strMensaje);
                    //objMQMensaje.MessageData = Trim(strMensaje)
                    //objMQMensaje.Persistence = psPersistencia
                    //objMQMensaje.Expiry = psExpirar
                    //objMQMensaje.ReplyToQueueManagerName = Ls_ReplayMQQueue;
                    QUEUE.Put(MSG, mqsMQOpciones);

                    CerrarColaMQ();
                }
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            return true;
        }
    }
}
