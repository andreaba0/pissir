using frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace frontend.Pages.AziendaAgricola
{
    public class StoricoDispositiviModel : PageModel
    {
        public List<SensoreUmiditaLog>? SensoriUmiditaLogs { get; set; }
        public List<SensoreTemperaturaLog>? SensoriTemperaturaLogs { get; set; }
        public List<AttuatoreLog>? AttuatoriLogs { get; set; }

        public async Task<IActionResult> OnGet()
        {
            /*
            // L'utente non è autenticato, reindirizzamento sulla pagina di login
            if (!IsUserAuth()) return RedirectToPage("/auth/SignIn");

            // Imposta il token
            ApiReq.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Cookies["AccessToken"]);

            // Ottieni il CF dell'utente loggato
            string codFiscale = User.FindFirst("sub")?.Value;

            // Chiamata alle API per ottenere i dati
            if (codFiscale != null)
            {
                ApiReq.utente = await ApiReq.GetUserDataFromApi(codFiscale, HttpContext);
                SensoriUmiditaLogs = await ApiReq.GetSensoriUmiditaFromApi(ApiReq.utente.PartitaIva, HttpContext);
                SensoriTemperaturaLogs = await ApiReq.GetSensoriTemperaturaFromApi(ApiReq.utente.PartitaIva, HttpContext);
                AttuatoriLogs = await ApiReq.GetAttuatoriFromApi(ApiReq.utente.PartitaIva, HttpContext);
            }
            else
            {
                return BadRequest();
            }
            */
            
            // Simulazione dati
            SensoriUmiditaLogs = GetSensoriUmiditaLogs();
            SensoriTemperaturaLogs = GetSensoriTemperaturaLogs();
            AttuatoriLogs = GetAttuatoriLogs();

            return Page();
        }


        // Controllo utente autenticato
        private bool IsUserAuth()
        {
            if (ApiReq.utente == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return false;
            }
            return true;
        }








        // Simulazione dati
        private List<SensoreUmiditaLog> GetSensoriUmiditaLogs()
        {
            return new List<SensoreUmiditaLog>
        {
            new SensoreUmiditaLog { Id = "1", Tipo = "Tipo1", Time = DateTime.Now.ToString(), Umidita = 60.5f },
            new SensoreUmiditaLog { Id = "2", Tipo = "Tipo2", Time = DateTime.Now.AddHours(-1).ToString(), Umidita = 55.2f },
            new SensoreUmiditaLog { Id = "3", Tipo = "Tipo3", Time = DateTime.Now.AddHours(-2).ToString(), Umidita = 70.1f }
        };
        }

        private List<SensoreTemperaturaLog> GetSensoriTemperaturaLogs()
        {
            return new List<SensoreTemperaturaLog>
        {
            new SensoreTemperaturaLog { Id = "1", Tipo = "Tipo1", Time = DateTime.Now.ToString(), Temperatura = 25.3f },
            new SensoreTemperaturaLog { Id = "2", Tipo = "Tipo2", Time = DateTime.Now.AddHours(-1).ToString(), Temperatura = 22.1f },
            new SensoreTemperaturaLog { Id = "3", Tipo = "Tipo3", Time = DateTime.Now.AddHours(-2).ToString(), Temperatura = 27.8f }
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
