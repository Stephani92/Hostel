using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{   
    [Route("api/[controller]")]
    [ApiController]
    [RequireHttps]
    public class TestController : Controller
    {
        
        [HttpPost("Checkout")]
        public JsonResult Checkout()
        {
            //URI de checkout.
            string uri = @"https://ws.sandbox.pagseguro.uol.com.br/v2/checkout";

            //Conjunto de parâmetros/formData.
            System.Collections.Specialized.NameValueCollection postData = new System.Collections.Specialized.NameValueCollection();
            postData.Add("email", "boechat.stephani@gmail.com");
            postData.Add("token", "395D9749B55348C7935876EFE6215A71");
            postData.Add("currency", "BRL");
            postData.Add("itemId1", "0001");
            postData.Add("itemDescription1", "ProdutoPagSeguroI");
            postData.Add("itemAmount1", "3.00");
            postData.Add("itemQuantity1", "1");
            postData.Add("itemWeight1", "200");
            postData.Add("reference", "REF1234");
            postData.Add("senderName", "Jose Comprador");
            postData.Add("senderAreaCode", "44");
            postData.Add("senderPhone", "999999999");
            postData.Add("senderEmail", "comprador@uol.com.br");
            postData.Add("shippingAddressRequired", "false");

            //String que receberá o XML de retorno.
            string xmlString = null;

            //Webclient faz o post para o servidor de pagseguro.
            using (WebClient wc = new WebClient())
            {
                //Informa header sobre URL.
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                //Faz o POST e retorna o XML contendo resposta do servidor do pagseguro.
                var result = wc.UploadValues(uri, postData);

                //Obtém string do XML.
                xmlString = Encoding.ASCII.GetString(result);
            }

            //Cria documento XML.
            XmlDocument xmlDoc = new XmlDocument();

            //Carrega documento XML por string.
            xmlDoc.LoadXml(xmlString);

            //Obtém código de transação (Checkout).
            var code = xmlDoc.GetElementsByTagName("code")[0];
            
            //Obtém data de transação (Checkout).
            var date = xmlDoc.GetElementsByTagName("date")[0];

            //Monta a URL para pagamento.
            var paymentUrl = string.Concat("https://sandbox.pagseguro.uol.com.br/v2/checkout/payment.html?code=", code.InnerText);

            //Retorna dados para HTML.
            return Json(paymentUrl);
        }

        /// <summary>
        /// Post Consulta Transação.
        /// </summary>
        /// <returns>Json com mensagem processada.</returns>
        [HttpPost]
        public JsonResult ConsultaTransacao()
        {
            //uri de consulta da transação.
            string uri = "https://ws.sandbox.pagseguro.uol.com.br/v3/transactions/FD9EF71B9C2747869E66BF7D36B0AB97?email=boechat.stephani@gmail.com&token=395D9749B55348C7935876EFE6215A71";

            //Classe que irá fazer a requisição GET.
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);

            //Método do webrequest.
            request.Method = "GET";

            //String que vai armazenar o xml de retorno.
            string xmlString = null;

            //Obtém resposta do servidor.
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                //Cria stream para obter retorno.
                using (Stream dataStream = response.GetResponseStream())
                {
                    //Lê stream.
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        //Xml convertido para string.
                        xmlString = reader.ReadToEnd();

                        //Cria xml document para facilitar acesso ao xml.
                        XmlDocument xmlDoc = new XmlDocument();

                        //Carrega xml document através da string com XML.
                        xmlDoc.LoadXml(xmlString);

                        //Busca elemento status do XML.
                        var status = xmlDoc.GetElementsByTagName("status")[0];

                        //Fecha reader.
                        reader.Close();

                        //Fecha stream.
                        dataStream.Close();

                        //Verifica status de retorno.
                        //3 = Pago.
                        if (status.InnerText == "3")
                        {
                            return Json("Pago.");
                        }
                        //Outros resultados diferentes de 3.
                        else
                        {
                            return Json("Pendente.");
                        }
                    }
                }
            }
        }
    }
}
