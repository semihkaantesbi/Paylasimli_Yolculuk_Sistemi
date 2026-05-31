using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace DataAccessLayer
{
    public class Veritabani
    {
        // Kendi MySQL root şifreni Pwd= kısmına yaz kingo
        private const string BaglantiMetni = "Server=localhost;Database=rideshare_db;Uid=root;Pwd=123456;";
        public static DataTable ProsedurCalistir(string prosedurAdi, MySqlParameter[] parametreler = null)
        {
            using (MySqlConnection baglanti = new MySqlConnection(BaglantiMetni))
            {
                using (MySqlCommand komut = new MySqlCommand(prosedurAdi, baglanti))
                {
                    komut.CommandType = CommandType.StoredProcedure;
                    
                    if (parametreler != null)
                    {
                        komut.Parameters.AddRange(parametreler);
                    }

                    using (MySqlDataAdapter adaptor = new MySqlDataAdapter(komut))
                    {
                        DataTable tablo = new DataTable();
                        adaptor.Fill(tablo);
                        return tablo;
                    }
                }
            }
        }
    }
}