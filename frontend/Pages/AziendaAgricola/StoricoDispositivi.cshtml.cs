using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace frontend.Pages.AziendaAgricola
{
    public class StoricoDispositiviModel : PageModel
    {
        public List<SensoreLog>? SensoriLogs { get; set; }
        public List<AttuatoreLog>? AttuatoriLogs { get; set; }

        public async Task<IActionResult> OnGet()
        {
            
            try
            {
                // Controllo utente autenticato
                if (!await ApiReq.IsUserAuth(HttpContext)) return RedirectToPage("/auth/SignIn");

                // Controllo utente autorizzato
                if (ApiReq.utente.Role!="FA") { return RedirectToPage("/DatiAccount"); }

                // Simulazione dati
                //SensoriLogs = GetSensoriLogs();
                //AttuatoriLogs = GetAttuatoriLogs();

                //return Page();

                // Richiesta API
                string data = await ApiReq.GetDataFromApi(HttpContext, "/object/sensor", true, true);
                SensoriLogs = JsonConvert.DeserializeObject<List<SensoreLog>>(data);

                data = await ApiReq.GetDataFromApi(HttpContext, "/object/actuator", true, true);
                AttuatoriLogs = JsonConvert.DeserializeObject<List<AttuatoreLog>>(data);

                return Page();
            }
            catch (HttpRequestException ex)
            {
                string statusCode = ex.Message.ToString().ToLower();

                if (statusCode == "unauthorized")
                {
                    // Errore 401
                    TempData["MessaggioErrore"] = "Non sei autorizzato. Effettua prima la richiesta di accesso ai servizi.";
                    return RedirectToPage("/Error");
                }
                else
                {
                    TempData["MessaggioErrore"] = $"Errore: {ex.Message}. Riprovare pi� tardi.";
                    return RedirectToPage("/Error");
                }
            }
        }


        // Simulazione dati
        private List<SensoreLog> GetSensoriLogs()
        {
            return new List<SensoreLog>
            {
                new SensoreLog { Id = "1", Tipo = "humidity", Time = DateTime.Now.ToString(), Misura = 60.5f },
                new SensoreLog { Id = "2", Tipo = "humidity", Time = DateTime.Now.AddHours(-1).ToString(), Misura = 55.2f },
                new SensoreLog { Id = "3", Tipo = "humidity", Time = DateTime.Now.AddHours(-2).ToString(), Misura = 70.1f },
                new SensoreLog { Id = "4", Tipo = "temperature", Time = DateTime.Now.ToString(), Misura = 25.3f },
                new SensoreLog { Id = "5", Tipo = "temperature", Time = DateTime.Now.AddHours(-1).ToString(), Misura = 22.1f },
                new SensoreLog { Id = "6", Tipo = "temperature", Time = DateTime.Now.AddHours(-2).ToString(), Misura = 27.8f }
            };
        }

        private List<AttuatoreLog> GetAttuatoriLogs()
        {
            return new List<AttuatoreLog>
        {
            new AttuatoreLog { Id = "1", Time = DateTime.Now.ToString(), Attivo = true },
            new AttuatoreLog { Id = "2", Time = DateTime.Now.AddHours(-1).ToString(), Attivo = false },
            new AttuatoreLog { Id = "3", Time = DateTime.Now.AddHours(-2).ToString(), Attivo = true }
        };
        }
    }

}
