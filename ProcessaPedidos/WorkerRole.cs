using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Api.Models;
using System.IO;
using Microsoft.Azure.NotificationHubs;

namespace ProcessaPedidos
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private CloudQueue cloudQueue;

        public WorkerRole()
        {
            //var connectionString = "UseDevelopmentStorage=true";
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=storagerc;AccountKey=4jaYJR7MEP6W4UjxI4ZNqJ5a+lw/yelS6e++QIg6Mvz5/UK74Dtj1K0ZxtHCBFiIvaqgk+UIwJOkzkbjNUPPUQ==";

            CloudStorageAccount cloudStorageAccount;

            if (!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
            {

            }

            var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
            cloudQueue = cloudQueueClient.GetQueueReference("quepedidos");

            // Note: Usually this statement can be executed once during application startup or maybe even never in the application.
            //       A queue in Azure Storage is often considered a persistent item which exists over a long time.
            //       Every time .CreateIfNotExists() is executed a storage transaction and a bit of latency for the call occurs.
            cloudQueue.CreateIfNotExists();
        }



        public override void Run()
        {
            Trace.TraceInformation("ProcessaPedidos is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("ProcessaPedidos foi iniciado.");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("ProcessaPedidos está parando...");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("ProcessaPedidos parado.");
        }

        
        private async Task RunAsync(CancellationToken cancellationToken)
        {

            String strPedSer = String.Empty;

            while (!cancellationToken.IsCancellationRequested)
            {
                var pedser = cloudQueue.GetMessage();

                if (pedser == null)
                    break;
                else
                {
                    strPedSer = pedser.AsString;
                    Pedido objped = JsonConvert.DeserializeObject<Pedido>(strPedSer);

                    printLog("Pedido: " + objped.idPedido, strPedSer);
                    TratarPedido(objped);

                    cloudQueue.DeleteMessage(pedser);
                    await Task.Delay(1000);
                }
                
            }



        }

        private void printLog(string Titulo, string strLog)
        {
            
            Trace.WriteLine("---------------------------------------------------");
            Trace.WriteLine("log:" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            Trace.WriteLine(Titulo);
            Trace.TraceInformation(strLog);
            Trace.WriteLine("---------------------------------------------------");
        }

        /// <summary>
        /// Metodo para tratamento de negócio
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool TratarPedido(Pedido obj)
        {
            //envio de notificaçãp
            try
            {

                var x = SendNotificationAsync("Pedido: " + obj.idPedido + " realizado com sucesso. ", "Cintia").Result;
                return true;
            }
            catch (Exception ex)
            {
                printLog("ERRO", "... Erro no pedido " + obj.idPedido.ToString()+ ".  " + ex.Message);
                return false;
            }


            
        }

        private static NotificationHubClient _hub;
        public async Task<bool> SendNotificationAsync(string message, string to_tag)
        {
            string[] userTag = new string[1];
            userTag[0] = to_tag;

            string defaultFullSharedAccessSignature = "Endpoint=sb://hbscr13.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=hS+NQ17Qu8CwNBVxzSWVY3ORJ4nK2lMNTFENbFc5Vck=";
            string hubName = "hbPedidos";
            _hub = NotificationHubClient.CreateClientFromConnectionString(defaultFullSharedAccessSignature, hubName);

            NotificationOutcome outcome = null;

            // Android
            var notif = "{ \"data\" : {\"message\":\"" + message + "\"}}";
            outcome = await _hub.SendGcmNativeNotificationAsync(notif, userTag);

            if (outcome != null)
            {
                if (!((outcome.State == NotificationOutcomeState.Abandoned) ||
                    (outcome.State == NotificationOutcomeState.Unknown)))
                {
                    return true;
                }
            }
            return false;
        }

    }
}


/*
 * 
 *             while (!cancellationToken.IsCancellationRequested)
            {

                var msq = cloudQueue.GetMessage();
                if (msq == null)
                    return;

                string strPedido = msq.AsString;

                Pedido pedido = (Pedido)JsonConvert.DeserializeObject(strPedido);


                string strResult = JsonConvert.SerializeObject(pedido);

                Trace.TraceInformation(strPedido);

                cloudQueue.DeleteMessage(msq);

                await Task.Delay(1000);

                //// TODO: Replace the following with your own logic.


            }
*/