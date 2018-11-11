using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET;
using Alexa.NET.LocaleSpeech;
using System.Collections.Generic;

namespace Company.Function
{
    public static class Alexa
    {

        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string        json         = await req.ReadAsStringAsync();
            var           skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);
            string        language     = skillRequest.Request.Locale;
            var           requestType  = skillRequest.GetRequestType();
            var           locale       = SetupLanguages(skillRequest);
            SkillResponse response     = null;


            if(requestType==typeof(LaunchRequest)) 
            {
                var message = await locale.Get("WELCOME", null);
                response = ResponseBuilder.Tell(message);
                response.Response.ShouldEndSession = false;
            }
            else if (requestType == typeof(SessionEndedRequest))
            {
                log.LogInformation("Session ended");
                response = ResponseBuilder.Empty();
                response.Response.ShouldEndSession = true;                
            }
            else if (requestType == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;

                if (intentRequest.Intent.Name == "GENERATE_NUMBER")
                {
                    var random  = new System.Random();
                    var number  = random.Next(100).ToString();
                    var message = await locale.Get("GENERATE_NUMBER",  new string[] { number });
                    response = ResponseBuilder.Tell(message);
                    response.Response.ShouldEndSession = false;
                }                
                else
                {
                    var message = await locale.Get(intentRequest.Intent.Name, null);
                    response = ResponseBuilder.Tell(message);
                    response.Response.ShouldEndSession = false;
                }
            }


            return new OkObjectResult(response);
        }

        public static ILocaleSpeech SetupLanguages(SkillRequest skillRequest)
        {
            var store = new DictionaryLocaleSpeechStore();
            store.AddLanguage("en", new Dictionary<string, object>
            {
                { "WELCOME",             "Welcome to the my demo skill"       },
                { "THANK_YOU_INTENT",    "You are welcome!"                   },
                { "GENERATE_NUMBER",     "The next random number is {0}"      },
                { "AMAZON.HelpIntent",   "Ask me to generate a random number" },
                { "AMAZON.CancelIntent", "I'm stopping the request"           },
                { "AMAZON.StopIntent",   "Bye bye"                            }
            });


            store.AddLanguage("it", new Dictionary<string, object>
            {
                { "WELCOME",             "Benvenuto nello skill di prova"                },
                { "THANK_YOU_INTENT",    "Prego"                                         },
                { "GENERATE_NUMBER",     "Il prossimo numero random è {0}"               },
                { "AMAZON.HelpIntent",   "Puoi chiedermi di generarti un numero random"  },
                { "AMAZON.CancelIntent", "Sto interrompendo la richiesta"                },
                { "AMAZON.StopIntent",   "Ciao e grazie"                                 }
            });

            var localeSpeechFactory = new LocaleSpeechFactory(store);
            var locale = localeSpeechFactory.Create(skillRequest);

            return locale;
        }
    }
}
