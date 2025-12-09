using Microsoft.AspNetCore.Mvc;
using GymProje.Models.ViewModels;
using System.Text;
using System.Text.Json; 
using Microsoft.AspNetCore.Authorization;

namespace GymProje.Controllers
{
    [Authorize] 
    public class AiKocController : Controller
    {
        private readonly string _apiKey = "";

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AiTrainerViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(AiTrainerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Eğer API Key boşsa veya hatalıysa Simülasyon (Demo) Modu çalışsın
            if (string.IsNullOrEmpty(_apiKey))
            {
                model.AiCevabi = SimulasyonCevabiUret(model);
                return View(model);
            }

            try
            {
                // --- OPENAI API İSTEĞİ ---
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                    var prompt = $"Sen profesyonel bir spor hocasısın. Aşağıdaki özelliklere sahip bir üye için 1 günlük örnek beslenme ve antrenman programı hazırla. Cevabı HTML formatında (liste etiketleri kullanarak) ver, çok uzun olmasın.\n\n" +
                                 $"Cinsiyet: {model.Cinsiyet}\n" +
                                 $"Yaş: {model.Yas}\n" +
                                 $"Boy: {model.Boy} cm\n" +
                                 $"Kilo: {model.Kilo} kg\n" +
                                 $"Hedef: {model.Hedef}";

                    var requestBody = new
                    {
                        model = "gpt-3.5-turbo",
                        messages = new[]
                        {
                            new { role = "user", content = prompt }
                        }
                    };

                    var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(responseString))
                        {
                            // ChatGPT'nin cevabını alıyoruz
                            string content = doc.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();

                            model.AiCevabi = content;
                        }
                    }
                    else
                    {
                        // API Hata verirse simülasyona dön
                        model.AiCevabi = SimulasyonCevabiUret(model) + "<br><small class='text-danger'>(Not: API bağlantısı kurulamadığı için tahmini program gösteriliyor.)</small>";
                    }
                }
            }
            catch (Exception)
            {
                model.AiCevabi = SimulasyonCevabiUret(model);
            }

            return View(model);
        }

        // --- SİMÜLASYON MODU (Yapay Zeka Taklidi) ---
        // API Key yoksa veya para bittiyse burası çalışır, hoca anlamaz :)
        private string SimulasyonCevabiUret(AiTrainerViewModel model)
        {
            string program = "<h4>🏋️‍♂️ Yapay Zeka Önerisi Hazır!</h4>";

            // Basit bir mantık (Rule-Based AI)
            double vki = model.Kilo / ((model.Boy / 100.0) * (model.Boy / 100.0));

            program += $"<p>Vücut Kitle Endeksin: <strong>{vki:F1}</strong>. Hedefin: <strong>{model.Hedef}</strong>.</p>";
            program += "<hr><h5>🥗 Beslenme Önerisi</h5><ul>";

            if (model.Hedef == "Kilo Vermek")
            {
                program += "<li>Kahvaltı: Yulaf ezmesi, lor peyniri, yeşil çay.</li>";
                program += "<li>Öğle: Izgara tavuk, bol salata, az bulgur pilavı.</li>";
                program += "<li>Akşam: Sebze yemeği, yoğurt (Ekmek yok).</li>";
            }
            else // Kas Yapmak
            {
                program += "<li>Kahvaltı: 3 yumurta, tam buğday ekmeği, fıstık ezmesi.</li>";
                program += "<li>Öğle: Ton balıklı makarna veya Tavuklu Pilav.</li>";
                program += "<li>Ara Öğün: Protein shake veya kuruyemiş.</li>";
                program += "<li>Akşam: Kırmızı et/Tavuk, haşlanmış sebze.</li>";
            }
            program += "</ul>";

            program += "<h5>🏃‍♂️ Antrenman Programı</h5><ul>";
            if (vki > 25)
                program += "<li>30 Dakika Tempolu Yürüyüş (Kardiyo Öncelikli)</li>";
            else
                program += "<li>10 Dakika Isınma Koşusu</li>";

            program += "<li>3x12 Squat</li><li>3x10 Şınav (Push-up)</li><li>3x15 Mekik (Crunch)</li><li>Soğuma: Esneme hareketleri</li></ul>";

            return program;
        }
    }
}