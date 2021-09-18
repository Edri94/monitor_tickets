using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorMQTKT.Funciones
{
    public class MqSeries
    {
        public MQQueueManager QMGR = null; //mqsManager
        public MQQueueManager QMGR1 = null;
        public MQQueue QUEUE = null; //MQQMonitorLectura
        public MQQueue QUEUE1 = null;
        public MQPutMessageOptions pmo = null;
        public MQMessage MSG = null;

        public string strMQManager;
        public string strMQQMonitorLectura;
        public string strMQQMonitorEscritura;

        public bool blnConectado;

        public Funcion funcion;


        public MqSeries()
        {
            funcion = new Funcion();
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
