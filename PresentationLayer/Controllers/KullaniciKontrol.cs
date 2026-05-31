using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using System.Data;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KullaniciKontrol : ControllerBase
    {
  
        public static int AktifKullaniciId = 1; 

     
        
        public class KayitIstegi
        {
            public string AdSoyad { get; set; }
            public string Email { get; set; }
            public string Sifre { get; set; }
        }

        public class AracKayitIstegi
        {
            public string Marka { get; set; }
            public string Model { get; set; }
            public string Plaka { get; set; }
            public string Renk { get; set; }
        }

        public class GirişIstegi
        {
            public string Email { get; set; }
            public string Sifre { get; set; }
        }

        public class YolculukIstegi
        {
            public int AracId { get; set; }
            public string KalkisYeri { get; set; }
            public string VarisYeri { get; set; }
            public System.DateTime TarihSaat { get; set; }
            public decimal Fiyat { get; set; }
            public int KoltukSayisi { get; set; }
        }


        [HttpPost("kayit")]
        public IActionResult Kayit([FromForm] KayitIstegi veri)
        {
            try
            {
                KullaniciServis.KullaniciKaydet(veri.AdSoyad, veri.Email, veri.Sifre);
                return Redirect("/api/KullaniciKontrol/sayfa");
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("Duplicate entry"))
                {
                    return BadRequest("Bu e-posta adresi sistemde zaten kayıtlı!");
                }
                return BadRequest(ex.Message);
            }
        }

       
        [HttpPost("giris")]
        public IActionResult Giris([FromForm] GirişIstegi veri)
        {
            try
            {
                DataTable dt = KullaniciServis.GirişYap(veri.Email, veri.Sifre);

                if (dt.Rows.Count > 0)
                {
                    // Giriş yapan kullanıcının gerçek ID'sini yakalayıp sisteme kilitliyoruz
                    AktifKullaniciId = System.Convert.ToInt32(dt.Rows[0]["id"]);
                    return Redirect("/anasayfa.html");
                }
                else
                {
                    return BadRequest("Hatalı e-posta veya şifre!");
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpPost("arac-kayit")]
        public IActionResult AracKayit([FromForm] AracKayitIstegi veri)
        {
            try
            {
                KullaniciServis.AracKaydet(AktifKullaniciId, veri.Marka, veri.Model, veri.Plaka, veri.Renk);
                // Araç kaydı başarılı olunca listeyi görsün diye sayfayı yeniliyoruz
                return Redirect("/api/KullaniciKontrol/arac-ekle-sayfasi");
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("Duplicate entry"))
                {
                    return BadRequest("Bu plaka ile zaten kayıtlı bir araç var!");
                }
                return BadRequest(ex.Message);
            }
        }

   
        [HttpPost("yolculuk-olustur")]
        public IActionResult YolculukOlustur([FromForm] YolculukIstegi veri)
        {
            try
            {
                KullaniciServis.YolculukOlustur(AktifKullaniciId, veri.AracId, veri.KalkisYeri, veri.VarisYeri, veri.TarihSaat, veri.Fiyat, veri.KoltukSayisi);
                return Ok("Yolculuk ilanı başarıyla oluşturuldu!");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

   
        [HttpPost("rezervasyon-yap")]
        public IActionResult RezervasyonYap([FromForm] int yolculukId)
        {
            try
            {
                KullaniciServis.RezervasyonYap(yolculukId, AktifKullaniciId);
                return Redirect("/api/KullaniciKontrol/yolculuklar-sayfa");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("yolculuk-iptal")]
        public IActionResult YolculukIptal([FromForm] int yolculukId)
        {
            try
            {
                KullaniciServis.YolculukIptal(yolculukId);
                return Redirect("/api/KullaniciKontrol/yolculuklar-sayfa");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

       
        [HttpGet("sayfa")]
        public IActionResult ListeleSayfasi()
        {
            try
            {
                DataTable dt = KullaniciServis.TumKullanicilariGetir();

                string html = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Kullanıcı Listesi</title>
                    <style>
                        body { font-family: Arial, sans-serif; padding: 50px; background-color: #f4f4f4; }
                        .container { max-width: 800px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
                        h2 { text-align: center; color: #333; }
                        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                        th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }
                        th { background-color: #007bff; color: white; }
                        tr:nth-child(even) { background-color: #f9f9f9; }
                        .btn { display: inline-block; padding: 10px 15px; background: #28a745; color: white; text-decoration: none; border-radius: 4px; margin-bottom: 20px; }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Sistemde Kayıtlı Kullanıcılar</h2>
                        <a href='/anasayfa.html' class='btn'>🏠 Ana Sayfaya Dön</a>
                        <table>
                            <tr>
                                <th>Ad Soyad</th>
                                <th>E-posta</th>
                            </tr>";

                foreach (DataRow row in dt.Rows)
                {
                    html += $@"
                            <tr>
                                <td>{row["ad_soyad"]}</td>
                                <td>{row["email"]}</td>
                            </tr>";
                }

                html += @"
                        </table>
                    </div>
                </body>
                </html>";

                return Content(html, "text/html", System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Listeleme hatası: {ex.Message}");
            }
        }

      
        [HttpGet("yolculuklar-sayfa")]
        public IActionResult YolculuklarSayfasi()
        {
            try
            {
                DataTable dt = KullaniciServis.TumYolculuklariGetir();

                string html = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Aktif Yolculuk İlanları</title>
                    <style>
                        body { font-family: Arial, sans-serif; padding: 50px; background-color: #f4f4f4; }
                        .container { max-width: 950px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
                        h2 { text-align: center; color: #333; }
                        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                        th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }
                        th { background-color: #ffc107; color: #212529; }
                        tr:nth-child(even) { background-color: #f9f9f9; }
                        .btn { display: inline-block; padding: 10px 15px; background: #007bff; color: white; text-decoration: none; border-radius: 4px; margin-bottom: 20px; }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>🚗 Aktif Yolculuk İlanları</h2>
                        <a href='/anasayfa.html' class='btn'>🏠 Ana Sayfaya Dön</a>
                        <table>
                            <tr>
                                <th>Sürücü</th>
                                <th>Araç Bilgisi</th>
                                <th>Kalkış</th>
                                <th>Varış</th>
                                <th>Tarih & Saat</th>
                                <th>Fiyat (TL)</th>
                                <th>Boş Koltuk</th>
                                <th>İşlem</th>
                            </tr>";

                foreach (DataRow row in dt.Rows)
                {
                    System.DateTime tarih = System.Convert.ToDateTime(row["tarih_saat"]);
                    
                  
                    int surucuId = System.Convert.ToInt32(row["kullanici_id"]);

                    html += $@"
                            <tr>
                                <td>{row["surucu_adi"]}</td>
                                <td>{row["arac_bilgisi"]}</td>
                                <td>{row["kalkis_yeri"]}</td>
                                <td>{row["varis_yeri"]}</td>
                                <td>{tarih.ToString("dd.MM.yyyy HH:mm")}</td>
                                <td>{row["fiyat"]} TL</td>
                                <td>{row["koltuk_sayisi"]}</td>
                                <td>";

                 
                    if (surucuId == AktifKullaniciId)
                    {
                        html += $@"
                                    <form action='/api/KullaniciKontrol/yolculuk-iptal' method='POST' style='margin:0;'>
                                        <input type='hidden' name='yolculukId' value='{row["id"]}'>
                                        <button type='submit' style='background:#dc3545; color:white; border:none; padding:5px 10px; border-radius:4px; cursor:pointer;'>İlanı Sil</button>
                                    </form>";
                    }
                    else
                    {
                        html += $@"
                                    <form action='/api/KullaniciKontrol/rezervasyon-yap' method='POST' style='margin:0;'>
                                        <input type='hidden' name='YolculukId' value='{row["id"]}'>
                                        <button type='submit' style='background:#28a745; color:white; border:none; padding:5px 10px; border-radius:4px; cursor:pointer;'>Koltuk Ayırt</button>
                                    </form>";
                    }

                    html += "</td></tr>";
                }

                html += @"
                        </table>
                    </div>
                </body>
                </html>";

                return Content(html, "text/html", System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Yolculuk listeleme hatası: {ex.Message}");
            }
        }
        [HttpGet("arac-ekle-sayfasi")]
        public IActionResult AracEkleSayfasi()
        {
            try
            {
                
                DataTable dt = KullaniciServis.KullaniciAraclariniGetir(AktifKullaniciId);

                string html = @"
                <!DOCTYPE html>
                <html lang='tr'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Araç Kayıt ve Listem</title>
                    <style>
                        body { font-family: Arial, sans-serif; padding: 40px; background-color: #f4f4f4; }
                        .main-container { max-width: 600px; margin: 0 auto; }
                        .form-container { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); margin-bottom: 30px; }
                        h2, h3 { text-align: center; color: #333; margin-top: 0; }
                        input { width: 100%; padding: 10px; margin-bottom: 15px; display: block; box-sizing: border-box; border: 1px solid #ccc; border-radius: 4px; }
                        button { width: 100%; padding: 10px; background-color: #28a745; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 16px; }
                        button:hover { background-color: #218838; }
                        table { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
                        th, td { border: 1px solid #ddd; padding: 10px; text-align: left; }
                        th { background-color: #28a745; color: white; }
                        tr:nth-child(even) { background-color: #f9f9f9; }
                        .id-badge { background: #e2e3e5; padding: 3px 8px; border-radius: 4px; font-weight: bold; color: #383d41; }
                        .lnk { display: block; text-align: center; margin-top: 15px; color: #007bff; text-decoration: none; }
                    </style>
                </head>
                <body>
                    <div class='main-container'>
                        <div class='form-container'>
                            <h2>🚘 Yeni Araç Kaydet</h2>
                            <form action='/api/KullaniciKontrol/arac-kayit' method='POST'>
                                <input type='text' name='Marka' placeholder='Araç Markası (Örn: Toyota)' required>
                                <input type='text' name='Model' placeholder='Araç Modeli (Örn: Corolla)' required>
                                <input type='text' name='Plaka' placeholder='Araç Plakası (Örn: 55ABC55)' required>
                                <input type='text' name='Renk' placeholder='Araç Rengi (Örn: Beyaz)'>
                                <button type='submit'>Aracı Kaydet</button>
                            </form>
                            <a href='/anasayfa.html' class='lnk'>🏠 Ana Sayfaya Dön</a>
                        </div>

                        <h3>🔑 Benim Araçlarım ve ID Bilgileri</h3>
                        <table>
                            <tr>
                                <th>Araç ID</th>
                                <th>Marka / Model</th>
                                <th>Plaka</th>
                            </tr>";

                if (dt.Rows.Count == 0)
                {
                    html += "<tr><td colspan='3' style='text-align:center; color:#888;'>Henüz kayıtlı bir aracınız yok.</td></tr>";
                }
                else
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        html += $@"
                                <tr>
                                    <td><span class='id-badge'>ID: {row["id"]}</span></td>
                                    <td>{row["marka"]} {row["model"]}</td>
                                    <td>{row["plaka"]}</td>
                                </tr>";
                    }
                }

                html += @"
                        </table>
                    </div>
                </body>
                </html>";

                return Content(html, "text/html", System.Text.Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Araç sayfası yüklenirken hata oluştu: {ex.Message}");
            }
        }
    }
}