using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Data;
using Microsoft.Data.SqlClient;
using ModernWpfApp.Services;
using System.Linq.Expressions;

namespace BoardGameFrontend.AjramData
{
    public static class AjramStaticData
    {
        private static bool m_bInitialized = false;
        private static ConnectionService Connection => ConnectionService.Instance;
        public static List<ADBModel> adb_ModeleKrzesel = new List<ADBModel>();
        public static List<ADBModel> adb_ModeleStolow = new List<ADBModel>();
        public static List<ADBDrewno> adb_Drewno = new List<ADBDrewno>();
        public static List<ADBDetal> adb_Slizgacze = new List<ADBDetal>();
        public static List<ADBDetal> adb_Kolatka = new List<ADBDetal>();
        public static List<ADBDetal> adb_Gwozdziki = new List<ADBDetal>();
        public static List<ADBDetal> adb_Kolka = new List<ADBDetal>();
        public static List<ADBDetal> adb_Etykieta = new List<ADBDetal>();
        public static List<ADBDetal> adb_Brendowanie = new List<ADBDetal>();
        public static List<ADBDetal> adb_Brylanciki = new List<ADBDetal>();
        public static List<ADBWaluta> adb_Waluty = new List<ADBWaluta>();
        public static List<ADBDostawcaTkaniny> adb_DostawcyTkanin = new List<ADBDostawcaTkaniny>();
        public static List<ADBFirma> adb_Klienci = new List<ADBFirma>();
        public static List<ADBFirmaAdres> adb_KlienciAdresy = new List<ADBFirmaAdres>();
        public static List<ADBKolor> adb_Kolory = new List<ADBKolor>();
        static AjramStaticData()
        {

        }

        public static void DoInitializeAjramData()
        {
            if(m_bInitialized)
                return;

            m_bInitialized = true;

            DataSet dataSet = GetDataFromDBQuery("SELECT * FROM dbo.Modele_Krzesla_Drewniane ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_ModeleKrzesel.Add(new ADBModel(row));
            }

            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.Modele_Stoly_Drewniane ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_ModeleStolow.Add(new ADBModel(row));
            }

            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.Gatunek_Drewna ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Drewno.Add(new ADBDrewno(row));
            }
                    
            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.DetaleSlizgacz ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Slizgacze.Add(new ADBDetal(row));
            }

            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.DetaleKolatka ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Kolatka.Add(new ADBDetal(row));
            }

            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.DetaleGwozdziki ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Gwozdziki.Add(new ADBDetal(row));
            }
                    
            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.DetaleKolka ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Kolka.Add(new ADBDetal(row));
            } 

            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.DetaleEtykieta ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Etykieta.Add(new ADBDetal(row));
            }   
            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.DetaleBrendowanie ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Brendowanie.Add(new ADBDetal(row));
            }   
            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.DetaleBrylanciki ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Brylanciki.Add(new ADBDetal(row));
            }

            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.Waluty ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Waluty.Add(new ADBWaluta(row));
            }
// NOT is failing, <> = 0 , -1 is working; <> = true false is failing
            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.Tkaniny_Dostawcy WHERE Ignore = 0 ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_DostawcyTkanin.Add(new ADBDostawcaTkaniny(row));
            }

            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.Klienci ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Klienci.Add(new ADBFirma(row));
            }

            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.Klienci_Adresy ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_KlienciAdresy.Add(new ADBFirmaAdres(row));
            }

            dataSet = GetDataFromDBQuery("SELECT * FROM dbo.Kolory ORDER BY ID;");
            if(dataSet != null)
            {
                foreach (DataRow row in dataSet.Tables["MyQuery"].Rows)
                    adb_Kolory.Add(new ADBKolor(row));
            }
        }

        public static void AddAdresFirmy(string adres, int id)
        {
            var fa = new ADBFirmaAdres(id, adres);
            adb_KlienciAdresy.Add(fa);
        }
        public static DataSet GetDataFromDBQuery(string query, string hName = "MyQuery")
        {    
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                var conn = Connection.GetConnection();
                conn.Open();
                
                SqlCommand cmd = new SqlCommand(query, conn);
                adapter.SelectCommand = cmd;
                adapter.Fill(ds, "MyQuery");

                conn.Close(); 
                return ds;
            }
            catch(Exception e){
                return ds;
            }
        }
        public static async Task<object> GetObjectFromScalar(string sql)
        {
            if (!Connection.IsConnected)
                throw new InvalidOperationException("Local database is not connected.");

            try
            {
                using (var connection = Connection.GetConnection())
                {
                    await connection.OpenAsync();

                    object rtnValue;
                    try
                    {
                        var SQLCommand = new SqlCommand();
                        SQLCommand.CommandType = CommandType.Text;
                        SQLCommand.Connection = connection;
                        SQLCommand.CommandText = sql;
                        rtnValue = SQLCommand.ExecuteScalar();
                        
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"System Error: {e.Message}");
                    }

                    connection.Close();
                    return rtnValue;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"System Error: {e.Message}");
            }
        }
        public static async Task<bool> IsQueryEmpty(string sql, object parameter)
        {

            if (!Connection.IsConnected)
                throw new InvalidOperationException("Local database is not connected.");

            try
            {
                using (var connection = Connection.GetConnection())
                {
                    await connection.OpenAsync();

                    object? rtnValue;
                    try
                    {
                        var SQLCommand = new SqlCommand();
                        SQLCommand.CommandType = CommandType.Text;
                        SQLCommand.Connection = connection;
                        SQLCommand.CommandText = sql;
                        SQLCommand.Parameters.AddWithValue("@param1",parameter);
                        rtnValue = SQLCommand.ExecuteScalar();
                        
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"System Error: {e.Message}");
                    }

                    connection.Close();
                    return rtnValue == null;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"System Error: {e.Message}");
            }
        }
        public static async Task<string> GetDateFromServer(string sFormat = "yyyy-MM-dd HH:mm:ss")
        {
            if (!ConnectionService.Instance.IsConnected)
                throw new InvalidOperationException("Local database is not connected.");

            try
            {
                using (var connection = ConnectionService.Instance.GetConnection())
                {
                    await connection.OpenAsync();

                    string sData = "";
                    try
                    {
                        var SQLCommand = new SqlCommand();
                        SQLCommand.CommandType = CommandType.Text;
                        SQLCommand.Connection = connection;
                        SQLCommand.CommandText = "SELECT FORMAT(GetDate(),'" + sFormat + "') as CurrentTime;";
                        sData = (string) SQLCommand.ExecuteScalar();
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"System Error: {e.Message}");
                    }

                    connection.Close();
                    return sData;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"System Error: {e.Message}");
            }
        }

        public static string GetWoodNameById(int id)
        {
            var f = adb_Drewno.FirstOrDefault(d => d.ID == id);
            if(f != null)
                return f.Gatunek;

            return "";
        }
        public static string GetSliderNameById(int id)
        {
            var f = adb_Slizgacze.FirstOrDefault(d => d.ID == id);
            if(f != null)
                return f.Typ;

            return "";
        }
    }
}