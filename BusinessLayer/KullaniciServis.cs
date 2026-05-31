using System;
using System.Data;
using DataAccessLayer;
using MySql.Data.MySqlClient;

namespace BusinessLayer
{
    public class KullaniciServis
    {
        public static void KullaniciKaydet(string adSoyad, string email, string sifre)
        {
            // İş Kuralları (Giriş Kontrolleri)
            if (string.IsNullOrEmpty(adSoyad) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sifre))
            {
                throw new Exception("Lütfen tüm alanları doldurun.");
            }

            if (!email.Contains("@"))
            {
                throw new Exception("Geçersiz e-mail adresi.");
            }

            // Parametreleri MySQL formatına bağla ve DAL katmanını tetikle
            MySqlParameter[] parametreler = new MySqlParameter[]
            {
                new MySqlParameter("p_ad_soyad", adSoyad),
                new MySqlParameter("p_email", email),
                new MySqlParameter("p_sifre", sifre)
            };

            Veritabani.ProsedurCalistir("sp_InsertUser", parametreler);
        }

        public static DataTable TumKullanicilariGetir()
        {
            return Veritabani.ProsedurCalistir("sp_GetAllUsers");
        }public static void AracKaydet(int kullaniciId, string marka, string model, string plaka, string renk)
        {
            // İş Kuralları
            if (kullaniciId <= 0 || string.IsNullOrEmpty(marka) || string.IsNullOrEmpty(model) || string.IsNullOrEmpty(plaka))
            {
                throw new Exception("Lütfen zorunlu araç alanlarını doldurun.");
            }

            MySqlParameter[] parametreler = new MySqlParameter[]
            {
                new MySqlParameter("p_kullanici_id", kullaniciId),
                new MySqlParameter("p_marka", marka),
                new MySqlParameter("p_model", model),
                new MySqlParameter("p_plaka", plaka),
                new MySqlParameter("p_renk", renk)
            };

            Veritabani.ProsedurCalistir("sp_InsertVehicle", parametreler);
        }public static DataTable GirişYap(string email, string sifre)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sifre))
            {
                throw new Exception("E-posta ve şifre boş bırakılamaz.");
            }

            MySqlParameter[] parametreler = new MySqlParameter[]
            {
                new MySqlParameter("p_email", email),
                new MySqlParameter("p_sifre", sifre)
            };

            // Eşleşen kullanıcı varsa DataTable içinde satır dönecek
            return Veritabani.ProsedurCalistir("sp_LoginControl", parametreler);
        }public static void YolculukOlustur(int kullaniciId, int aracId, string kalkis, string varis, System.DateTime tarih, decimal fiyat, int koltuk)
        {
            if (string.IsNullOrEmpty(kalkis) || string.IsNullOrEmpty(varis) || fiyat <= 0 || koltuk <= 0)
            {
                throw new System.Exception("Lütfen geçerli yolculuk bilgileri girin.");
            }

            MySqlParameter[] parametreler = new MySqlParameter[]
            {
                new MySqlParameter("p_kullanici_id", kullaniciId),
                new MySqlParameter("p_arac_id", aracId),
                new MySqlParameter("p_kalkis_yeri", kalkis),
                new MySqlParameter("p_varis_yeri", varis),
                new MySqlParameter("p_tarih_saat", tarih),
                new MySqlParameter("p_fiyat", fiyat),
                new MySqlParameter("p_koltuk_sayisi", koltuk)
            };

            Veritabani.ProsedurCalistir("sp_InsertRide", parametreler);
        }public static DataTable TumYolculuklariGetir()
        {
            // Veritabanındaki tüm aktif yolculuk ilanlarını döner
            return Veritabani.ProsedurCalistir("sp_GetAllRides");
        }public static void RezervasyonYap(int yolculukId, int yolcuId)
        {
            if (yolculukId <= 0 || yolcuId <= 0)
            {
                throw new System.Exception("Geçersiz rezervasyon bilgileri.");
            }

            MySqlParameter[] parametreler = new MySqlParameter[]
            {
                new MySqlParameter("p_yolculuk_id", yolculukId),
                new MySqlParameter("p_yolcu_id", yolcuId)
            };

            // Rezervasyon prosedürünü tetikliyoruz
            Veritabani.ProsedurCalistir("sp_BookSeat", parametreler);
        }public static void RezervasyonIptal(int rezervasyonId)
        {
            MySqlParameter[] parametreler = new MySqlParameter[] { new MySqlParameter("p_rezervasyon_id", rezervasyonId) };
            Veritabani.ProsedurCalistir("sp_CancelBooking", parametreler);
        }

        public static void YolculukIptal(int yolculukId)
        {
            MySqlParameter[] parametreler = new MySqlParameter[] { new MySqlParameter("p_yolculuk_id", yolculukId) };
            Veritabani.ProsedurCalistir("sp_CancelRide", parametreler);
        }public static DataTable KullaniciAraclariniGetir(int kullaniciId)
        {
            // Düz sorgu yerine MySQL'deki prosedürü parametreyle tetikliyoruz
            MySqlParameter[] parametreler = new MySqlParameter[]
            {
                new MySqlParameter("p_kullanici_id", kullaniciId)
            };

            return Veritabani.ProsedurCalistir("sp_GetUserVehicles", parametreler);
        }
    }
}