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

namespace BoardGameFrontend.AjramData
{
    public class ADBModel
    {
        public int ID {get; set;}
        public string Model {get; set;} = "";
        public bool AktualneDane {get; set;}
        public ADBModel(DataRow row)
        {
            ID = (int) row["ID"];
            Model = (string) row["Model"];
            AktualneDane = (bool) row["AktualneDane"];
        }
    }
    public class ADBDrewno
    {
        public int ID {get; set;}
        public string Gatunek {get; set;} = "";
        public string EngName {get; set;} = "";
        public ADBDrewno(DataRow row)
        {
            ID = (int) row["ID"];
            Gatunek = (string) row["Gatunek"];
            if(!row.IsNull("EngName")) 
                EngName = (string) row["EngName"];
        }
    }
    public class ADBDetal
    {
        public int ID {get; set;}
        public string Typ {get; set;} = "";
        public string TypEng {get; set;} = "";
        public ADBDetal(DataRow row)
        {
            ID = (int) row["ID"];
            Typ = (string) row["Typ"];
            if(row.Table.Columns.Contains("TypEng") && !row.IsNull("TypEng")) 
                TypEng = (string) row["TypEng"];
        }
    }
    public class ADBWaluta
    {
        public int ID {get; set;}
        public string Symbol {get; set;} = "";
        public ADBWaluta(DataRow row)
        {
            ID = (int) row["ID"];
            Symbol = (string) row["Symbol"];
        }
    }

    public class ADBDostawcaTkaniny
    {
        public int ID {get; set;}
        public string Nazwa {get; set;} = "";
        public ADBDostawcaTkaniny(DataRow row)
        {
            ID = (int) row["ID"];
            Nazwa = (string) row["Nazwa"];
        }
    }
    public class ADBFirma
    {
        public int ID {get; set;}
        public string Firma {get; set;} = "";
        public string EAccount {get; set;} = "";
        public ADBFirma(DataRow row)
        {
            ID = (int) row["ID"];
            Firma = (string) row["Firma"];
            if(row.Table.Columns.Contains("EAccount") && !row.IsNull("EAccount")) 
                EAccount = (string) row["EAccount"];
        }
    }
    public class ADBFirmaAdres
    {
        public int KlientID {get; set;}
        public string AdresDostawy {get; set;} = "";
        public ADBFirmaAdres(DataRow row)
        {
            KlientID = (int) row["KlientID"];
            AdresDostawy = (string) row["AdresDostawy"];
        }
        public ADBFirmaAdres(int id, string adres)
        {
            KlientID = id;
            AdresDostawy = adres;
        }
    }
    public class ADBKolor
    {
        public int ID {get; set;}
        public string Kolor {get; set;} = "";
        public string KolorEng {get; set;} = "";
        public bool Ciemny {get; set;} = false;
        public ADBKolor(DataRow row)
        {
            ID = (int) row["ID"];
            Kolor = (string) row["Kolor"];
            KolorEng = (string) row["KolorEng"];
            Ciemny = ((int) row["Ciemny"]) == 1;
        }
    }
}