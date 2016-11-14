using Api.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Api.Controllers
{
    //[Authorize]
    public class ValuesController : ApiController
    {
        static CloudQueue cloudQueue;

        private string getCnnStr()
        {
            string strch = "cnnStrDEV";

#if !DEBUG
            strch = "cnnStrPRD";
#endif
            return System.Configuration.ConfigurationManager.AppSettings[strch].ToString();

        }

        public ValuesController()
        {

            var connectionString = getCnnStr();
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



        // GET api/values
        public IEnumerable<string> Get()
        {
           return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public HttpResponseMessage Post(Pedido value)
        {
            try
            {
                if (value != null)
                {
                    string strProduto = JsonConvert.SerializeObject(value);
                    var message = new CloudQueueMessage(strProduto);
                    cloudQueue.AddMessage(message);
                }
                return Request.CreateResponse(HttpStatusCode.OK, value.idPedido, "application/json");
            }
            catch (Exception ex)
            {
                ResponseMessage(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message, "application/json");
            }
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}

/*
    //Estrutura do Pedido

    {
	"idPedido":1,
	"idCliente":1,
	"DataHora":"2016-11-08T04:20:49.0225485-02:00",
	"itens":[
		{
			"id":1,
			"descricao":"Ventilador ARNO",
			"valor":120.50
		},
		{
			"id":2,
			"descricao":"Impressora HP",
			"valor":310.50
		},
		{
			"id":3,
			"descricao":"SmartTV LG",
			"valor":1140.75
		}
			],
	"valortotal":1571.75
}

Instancia do Pedido para Testes

Pedido ped = new Pedido();
ped.idCliente = 1;
ped.idPedido = 1;
ped.DataHora = DateTime.Now;
ped.itens = new List<itempedido>();
ped.itens.Add( new itempedido(1,"Ventilador ARNO",120.50M));
ped.itens.Add(new itempedido(2, "Impressora HP", 310.50M));
ped.itens.Add(new itempedido(3, "SmartTV LG", 1140.75M));
string str = JsonConvert.SerializeObject(ped);


    */

