using System.Collections.ObjectModel;
using System.ComponentModel;

using System.Windows;
using System.Windows.Input;
using ModernWpfApp.Services;
using System.Diagnostics;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using BoardGameFrontend.AjramData;
using System.Windows.Controls;
using System.ComponentModel.DataAnnotations;
using CustomExtensionMembers;
using System.Security.Cryptography;
using Microsoft.VisualBasic;

namespace ModernWpfApp
{
    public partial class NewOrderWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public static ObservableCollection<string> TypyProduktow { get; } = new ObservableCollection<string>(){"Krzesło", "Stół"};
        public static ObservableCollection<string> TypyPakowania { get; } = new ObservableCollection<string>(){"Karton", "Luzem"};
        public static ObservableCollection<string> TypyTkaniny { get; } = new ObservableCollection<string>(){"Brak", "Tkanina", "Skóra"};
        public static ObservableCollection<string> TypyOlej { get; } = new ObservableCollection<string>(){"Brak", "Olej", "Lakier", "Lakier - robot", "Surowe"};
        public ObservableCollection<string> cbKrzesla { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbStoly { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbDrewno { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbSlizgacze { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbKolatka { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbKolka { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbGwozdziki { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbBrendowanie { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbEtykieta { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbBrylanciki { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbWaluty { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbDostawcaTkaniny { get; } = new ObservableCollection<string>();
        public ObservableCollection<ADBFirma> cbKlienci { get; } = new ObservableCollection<ADBFirma>();
        public ObservableCollection<KlientWithOrderCount> cbKlienciByOrders { get; set; } = new ObservableCollection<KlientWithOrderCount>();
        public ObservableCollection<string> cbKolory { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> cbAdresy { get; } = new ObservableCollection<string>();
        private int EsteticaId = -1;
        private string CurrentEOrderId = "";
        private bool _bKrzeslaShow = true;
        public bool bShowKrzesla 
        {
            get => _bKrzeslaShow;
            set
            {
                if (_bKrzeslaShow != value)
                {
                    _bKrzeslaShow = value;
                    bShowStoly = !value;
                    OnPropertyChanged(nameof(bShowKrzesla));
                }
            }
        }

        private KlientWithOrderCount? _selectedEKlient;
        public KlientWithOrderCount? SelectedEKlient
        {
            get => _selectedEKlient;
            set
            {
                if (_selectedEKlient != value)
                {
                    _selectedEKlient = value;
                    if(value != null)
                    {
                        string sOrderId = OrdersStore.Instance.Orders.FirstOrDefault(f => f.User?.Name == _selectedEKlient.Firma)?.Id ?? "";
                        LetsGoWithEOrderIdAndCurrentEKlient(sOrderId);
                    }

                    OnPropertyChanged(nameof(SelectedEKlient));
                }
            }
        }
        
        private ADBFirma _selectedPodklient;
        public ADBFirma SelectedPodklient
        {
            get => _selectedPodklient;
            set
            {
                if (_selectedPodklient != value)
                {
                    _selectedPodklient = value;
                    cbAdresy.Clear();
                    if(value != null)
                        DoSetAdresy(value.ID);

                    OnPropertyChanged(nameof(SelectedPodklient));
                }
            }
        }
        private bool _bStolyShow = false;
        public bool bShowStoly 
        {
            get => _bStolyShow;
            set
            {
                if (_bStolyShow != value)
                {
                    _bStolyShow = value;
                    OnPropertyChanged(nameof(bShowStoly));
                }
            }
        }
        private int _totalOrderCount = 0;
        public int TotalOrderCount 
        {
            get => _totalOrderCount;
            set
            {
                if (_totalOrderCount != value)
                {
                    _totalOrderCount = value;
                    OnPropertyChanged(nameof(TotalOrderCount));
                }
            }
        }

        // Expose the singleton so XAML can bind to Connection.IsConnected
        
        public NewOrderWindow()
        {
            foreach(var k in AjramStaticData.adb_ModeleKrzesel.OrderBy(q => q.Model))
                cbKrzesla.Add(k.Model);

            foreach(var k in AjramStaticData.adb_ModeleStolow.OrderBy(q => q.Model))
                cbStoly.Add(k.Model);
                
            foreach(var k in AjramStaticData.adb_Drewno.OrderBy(q => q.Gatunek))
                cbDrewno.Add(k.Gatunek);
                
            foreach(var k in AjramStaticData.adb_Gwozdziki.OrderBy(q => q.Typ))
                cbGwozdziki.Add(k.Typ);
            foreach(var k in AjramStaticData.adb_Slizgacze.OrderBy(q => q.Typ))
                cbSlizgacze.Add(k.Typ);
            foreach(var k in AjramStaticData.adb_Kolatka.OrderBy(q => q.Typ))
                cbKolatka.Add(k.Typ);
            foreach(var k in AjramStaticData.adb_Kolka.OrderBy(q => q.Typ))
                cbKolka.Add(k.Typ);
            foreach(var k in AjramStaticData.adb_Brendowanie.OrderBy(q => q.Typ))
                cbBrendowanie.Add(k.Typ);
            foreach(var k in AjramStaticData.adb_Etykieta.OrderBy(q => q.Typ))
                cbEtykieta.Add(k.Typ);
            foreach(var k in AjramStaticData.adb_Brylanciki.OrderBy(q => q.Typ))
                cbBrylanciki.Add(k.Typ);
            foreach(var k in AjramStaticData.adb_Waluty.OrderBy(q => q.Symbol))
                cbWaluty.Add(k.Symbol);
            foreach(var k in AjramStaticData.adb_DostawcyTkanin.OrderBy(q => q.Nazwa))
                cbDostawcaTkaniny.Add(k.Nazwa);
            foreach(var k in AjramStaticData.adb_Klienci.OrderBy(q => q.Firma))
                cbKlienci.Add(k);
                
            foreach(var k in AjramStaticData.adb_Kolory.OrderBy(q => q.Kolor))
                cbKolory.Add(k.Kolor);
            
            DeclareEsteticaIdAndOtherAsyncs();


            InitializeComponent();
            DataContext = this;
        }
        public async void DeclareEsteticaIdAndOtherAsyncs()
        {
             EsteticaId = (int) await AjramStaticData.GetObjectFromScalar("SELECT ID FROM dbo.Klienci WHERE Firma = 'Estetica';");
            
            await OrdersStore.Instance.FetchPendingOrdersAsync();
            foreach(var eorder in OrdersStore.Instance.Orders)
            {
                var fl = cbKlienciByOrders.FirstOrDefault(k => k.Firma == eorder.User?.Name);
                if(fl != null)
                    fl.iCount++;
                else
                    cbKlienciByOrders.Add(new KlientWithOrderCount(){Firma = eorder.User?.Name, iCount = 1});
            }
            TotalOrderCount = OrdersStore.Instance.Orders.Count;
        }
        private void DoSetAdresy(int id)
        {
            foreach(var k in AjramStaticData.adb_KlienciAdresy.Where(a => a.KlientID == id))
                cbAdresy.Add(k.AdresDostawy);
        }
        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
        private void NextOrder_Click(object sender, RoutedEventArgs e)
        {
            GoForNextOrder();
        }
        private void GoForNextOrder()
        {
            bool bNext = false;
            string sFirst = "";
            string sKlient = "";
            foreach(var eorder in OrdersStore.Instance.Orders)
            {
                if(eorder.Id == CurrentEOrderId)
                    bNext = true;
                else if(bNext)
                {
                    sFirst = eorder.Id;
                    sKlient = eorder.User.Name;
                    break;
                }
                else if(sFirst == "")
                {
                    sFirst = eorder.Id;
                    sKlient = eorder.User.Name;
                }
            }
            if(sFirst != "")
            {
                if(SelectedEKlient == null || SelectedEKlient.Firma != sKlient)
                    SelectedEKlient = cbKlienciByOrders.FirstOrDefault(f => f.Firma == sKlient);
                else
                {
                    LetsGoWithEOrderIdAndCurrentEKlient(sFirst);
                }
            }
        }

        private async Task LetsGoWithEOrderIdAndCurrentEKlient(string orderId)
        {
            string? ajramOrderId = (string) await AjramStaticData.GetObjectFromScalar("SELECT Nr_zamówienia FROM dbo.Zamowienia WHERE IdEOrder = '" + (orderId ?? "xe?") + "';");
            if(ajramOrderId != null && ajramOrderId != "")
            {
                MessageBoxResult dialogResult = MessageBox.Show("Podane zamówienie powinno się już znajdować w bazie danych jako: " + ajramOrderId + ".\n\nCzy chcesz kontynuować?", "Powtórzenie?", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.No)
                {
                    MessageBoxResult dr2 = MessageBox.Show("Oznaczyć zamówienie jako wykonane?", "Powtórzenie?", MessageBoxButton.YesNo);
                    if(dr2 == MessageBoxResult.Yes)
                    {
                        bool bCompleted = await TryToCompleteEOrder(orderId ?? "", ajramOrderId);
                        ReduceOrderCountProperly();
                        CurrentEOrderId = "";
                        TxtEOrderId.Text = "";
                        GoForNextOrder();     
                    }
                    return;
                }
            }
            if(SelectedEKlient != null)
            {
                var klient = cbKlienci.FirstOrDefault(f => f.EAccount == SelectedEKlient.Firma);
                if(klient == null)
                {
                    MessageBoxResult dialogResult = MessageBox.Show("Wybrany klient nie jest poprawnie przypisany do bazy danych. Będzie trzeba określić go ręcznie z listy.\n\nCzy chcesz kontynuować?", "Nieznany klient", MessageBoxButton.YesNo);
                    if(dialogResult == MessageBoxResult.Yes)
                    {
                        //do something
                        // zatwierdzić jako zaakceptowane?
                    }
                    else if (dialogResult == MessageBoxResult.No)
                        return;
                }
                else
                    SelectedPodklient = klient;
            }
            CurrentEOrderId = orderId;
            TxtEOrderId.Text = orderId;
            
            var allOurData = OrdersStore.Instance.Orders.FirstOrDefault(f => f.Id == orderId);
            if(allOurData == null)
            {
                MessageBox.Show("Błąd. Nieznaleziono wybranego zamówienie.", "Error");
                return;
            }
            var itemData = allOurData.FurnitureInOrders[0];
            if(itemData == null)
            {
                MessageBox.Show("Błąd. Wybrane zamówienie nie posiada informacji o produkcie.", "Error");
                return;
            }

            // populate data

            TxtNotkaKlienta.Text = itemData.extrainfo ?? "";
            TxtKolor.Text = itemData.colour ?? "";
            TxtWysokosc.Text = itemData.height.ToString();
            TxtTkanina.Text = itemData.fabric ?? "";

            if(itemData.fabricType == "FABRIC")
                TxtTypTkaniny.SelectedIndex = 2;
            else if(itemData.fabricType == "LEATHER")
                TxtTypTkaniny.SelectedIndex = 1;
            else
                TxtTypTkaniny.SelectedIndex = 0;
  
            CheckPowierzonaNiestandardowa.IsChecked = itemData.fabricSend;

            if(itemData.productType == "TABLE")
            {
                TxtTypProduktu.SelectedIndex = 1;
                TxtComboModelStol.Text = itemData.furnitureName;
            }
            else
            {
                TxtTypProduktu.SelectedIndex = 0;
                TxtComboModelKrzeslo.Text = itemData.furnitureName;
            }

            TxtAdresDostawy.Text = itemData.address ?? "";
            TxtNrZamKlient.Text = itemData.userOrderId ?? "";
            TxtIlosc.Text = itemData.quantity.ToString();
            TxtComboDrewno.Text = AjramStaticData.GetWoodNameById(itemData.woodType);
            TxtComboSlizgacz.Text = AjramStaticData.GetSliderNameById(itemData.slider);
        }
        
        public async void RejectOrder_Click(object sender, RoutedEventArgs ev)
        {
            if(CurrentEOrderId != "" && (TxtEOrderId.Text ?? "") == CurrentEOrderId)
            {
                var os = OrdersStore.Instance.Orders.FirstOrDefault(f => f.Id == CurrentEOrderId);
                if(os == null)
                    return;

                MessageBoxResult dialogResult = MessageBox.Show("Czy na pewno chcesz odrzucić zamówienie?", "Potwierdzenie akcji", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.No)
                    return;

                bool bRejectedProperly = await OrdersStore.Instance.RejectOrderAsync(os);
                if(bRejectedProperly)
                {
                    ReduceOrderCountProperly();
                    CurrentEOrderId = "";
                    TxtEOrderId.Text = ""; 
                    GoForNextOrder();
                    MessageBox.Show("Operacja wykonana pomyślnie.", "Sukces");
                }   
                else
                {
                    MessageBox.Show("Operacja nie udała się.", "Błąd");
                }

            }
        }
        public void AddNewOrder_Click(object sender, RoutedEventArgs ev)
        {
            ExecuteOrder();
        }

        private async Task DoPrintTkaninaOnly()
        {
            try
            {
                using (var connection = ConnectionService.Instance.GetConnection())
                {
                    await connection.OpenAsync();


                    if(!string.IsNullOrEmpty(TxtTkanina.Text))
                    {
                        if(TxtTkanina.Text.Length > 2)
                        {
                            if(TxtTypTkaniny.SelectedIndex != 0)
                            {
                                decimal d = await EvaluateAmountOfTextureNeeded(TxtComboModelKrzeslo.Text, TxtTypProduktu.Text, GetIntOr(TxtIlosc), TxtTypTkaniny.SelectedIndex);
                                MessageBox.Show("Potrzebna tkanina: " + d.ToString());
                            }
                        }
                    }
            
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        private async Task ExecuteOrder()
        {
            if (!ConnectionService.Instance.IsConnected)
        	    throw new InvalidOperationException("Local database is not connected.");

            if(!PassedComboChecks())
                return;

            if(!PassedLength())
                return;

            if(!PassedAjramChecks())
                return;
            
            if(IsOrderNumberOutdated())
                return;

            
            //await DoPrintTkaninaOnly();
            //return;

            bool bProceedWithEOrders = false;
            
            try
            {
                using (var connection = ConnectionService.Instance.GetConnection())
                {
                    await connection.OpenAsync();

                    try
                    {
                        string sSql = "INSERT INTO dbo.Zamowienia (Data, Nr_zamówienia, Data_wysyłki, Indeks_Klient, Nazwa_Klient, Miejscowość_Klient, PodindeksKlient, Nr_zamówienia_klienta, Wyrób_nazwa, Zmiany_i_dodatki, Ilość, Gatunek_drewna, Kolor, Tkanina, Tkanina_dostawca, Uwagi, Adres_dostawy, Sciezka_produkcji, Niestandardowe, Pracownik, Typ, Rodzaj_Pakowania, DetaleSlizgacz, DetaleKolatka, DetaleGwozdziki, DetaleKolka, DetaleBrendowanie, DetaleEtykieta, DetaleBrylanciki, WartoscCena, Waluta, Rabat, DetaleBrendowanieInfo, TypOleju, TypTkaniny, ZamBuforowe, Opis_wyrobu, ZmianaH, IdEOrder)";
                        sSql += " VALUES (@Data, @Nr_zamówienia, @Data_wysyłki, @Indeks_Klient, @Nazwa_Klient, @Miejscowość_Klient, @PodindeksKlient, @Nr_zamówienia_klienta, @Wyrób_nazwa, @Zmiany_i_dodatki, @Ilość, @Gatunek_drewna, @Kolor, @Tkanina, @Tkanina_dostawca, @Uwagi, @Adres_dostawy, @Sciezka_produkcji, @Niestandardowe, @Pracownik, @Typ, @Rodzaj_Pakowania, @DetaleSlizgacz, @DetaleKolatka, @DetaleGwozdziki, @DetaleKolka, @DetaleBrendowanie, @DetaleEtykieta, @DetaleBrylanciki, @WartoscCena, @Waluta, @Rabat, @DetaleBrendowanieInfo, @TypOleju, @TypTkaniny, @ZamBuforowe, @Opis_wyrobu, @ZmianaH, @IdEOrder);";
                        var SQLCommand = new SqlCommand();
                        SQLCommand.CommandType = CommandType.Text;
                        SQLCommand.Connection = connection;
                        SQLCommand.CommandText = sSql;
                        
                        SQLCommand.Parameters.Add("@Data", SqlDbType.DateTime).Value =  await AjramStaticData.GetDateFromServer();
                        SQLCommand.Parameters.Add("@Nr_zamówienia", SqlDbType.NVarChar).Value = TxtNrZam.Text;
                        SQLCommand.Parameters.Add("@Data_wysyłki", SqlDbType.NVarChar).Value = TxtDataWysyłki.Text;
                        SQLCommand.Parameters.Add("@Indeks_Klient", SqlDbType.Int).Value = EsteticaId;
                        SQLCommand.Parameters.Add("@Nazwa_Klient", SqlDbType.NVarChar).Value = "Estetica";
                        SQLCommand.Parameters.Add("@Miejscowość_Klient", SqlDbType.NVarChar).Value = DBNull.Value;
                        SQLCommand.Parameters.Add("@PodindeksKlient", SqlDbType.Int).Value = SelectedPodklient.ID;
                        SQLCommand.Parameters.Add("@Nr_zamówienia_klienta", SqlDbType.NVarChar).Value = TxtNrZamKlient.Text;
                        if(TxtTypProduktu.Text == "Stół")
                        {
                            SQLCommand.Parameters.Add("@Wyrób_nazwa", SqlDbType.NVarChar).Value = TxtComboModelStol.Text;
                            SQLCommand.Parameters.Add("@Sciezka_produkcji", SqlDbType.NVarChar).Value = "Stoły";
                            SQLCommand.Parameters.Add("@Typ", SqlDbType.NVarChar).Value = "stol";
                        }
                        else
                        {
                            SQLCommand.Parameters.Add("@Wyrób_nazwa", SqlDbType.NVarChar).Value = TxtComboModelKrzeslo.Text;
                            SQLCommand.Parameters.Add("@Sciezka_produkcji", SqlDbType.NVarChar).Value = "Krzesła - Pasłęk";
                            SQLCommand.Parameters.Add("@Typ", SqlDbType.NVarChar).Value = "krzD";
                        }
                        SQLCommand.Parameters.Add("@Zmiany_i_dodatki", SqlDbType.NVarChar).Value = TxtZmianyIDodatki.Text;
                        SQLCommand.Parameters.Add("@Ilość", SqlDbType.Int).Value = GetIntOr(TxtIlosc);
                        SQLCommand.Parameters.Add("@Gatunek_drewna", SqlDbType.NVarChar).Value = TxtComboDrewno.Text;
                        SQLCommand.Parameters.Add("@Kolor", SqlDbType.NVarChar).Value = TxtKolor.Text;
                        SQLCommand.Parameters.Add("@Tkanina", SqlDbType.NVarChar).Value = TxtTkanina.Text;
                        SQLCommand.Parameters.Add("@Tkanina_dostawca", SqlDbType.NVarChar).Value = TxtComboDostawcaTkaniny.Text;
                        SQLCommand.Parameters.Add("@Uwagi", SqlDbType.NVarChar).Value = TxtUwagi.Text;
                        SQLCommand.Parameters.Add("@Adres_dostawy", SqlDbType.NVarChar).Value = TxtAdresDostawy.Text;
                        SQLCommand.Parameters.Add("@Niestandardowe", SqlDbType.Bit).Value = false;
                        SQLCommand.Parameters.Add("@Pracownik", SqlDbType.NVarChar).Value = "EOrder";
                        SQLCommand.Parameters.Add("@Rodzaj_Pakowania", SqlDbType.NVarChar).Value = TxtTypPakowania.Text.Left(1);
                        SQLCommand.Parameters.Add("@DetaleSlizgacz", SqlDbType.NVarChar).Value = TxtComboSlizgacz.Text;
                        SQLCommand.Parameters.Add("@DetaleKolatka", SqlDbType.NVarChar).Value = TxtComboKolatka.Text;
                        SQLCommand.Parameters.Add("@DetaleGwozdziki", SqlDbType.NVarChar).Value = TxtComboGwozdziki.Text;
                        SQLCommand.Parameters.Add("@DetaleKolka", SqlDbType.NVarChar).Value = TxtComboKolka.Text;
                        SQLCommand.Parameters.Add("@DetaleBrendowanie", SqlDbType.NVarChar).Value = TxtComboBrendowanie.Text;
                        SQLCommand.Parameters.Add("@DetaleEtykieta", SqlDbType.NVarChar).Value = TxtComboEtykieta.Text;
                        SQLCommand.Parameters.Add("@DetaleBrylanciki", SqlDbType.NVarChar).Value = TxtComboBrylanciki.Text;
                        decimal.TryParse(TxtWartosc.Text, out decimal dWartosc);
                        SQLCommand.Parameters.Add("@WartoscCena", SqlDbType.Decimal).Value = dWartosc; 
                        SQLCommand.Parameters.Add("@Waluta", SqlDbType.NVarChar).Value = TxtComboWaluta.Text;
                        SQLCommand.Parameters.Add("@Rabat", SqlDbType.Int).Value = GetIntOr(TxtRabat);
                        SQLCommand.Parameters.Add("@DetaleBrendowanieInfo", SqlDbType.NVarChar).Value = TxtBrendowanie.Text;
                        SQLCommand.Parameters.Add("@TypOleju", SqlDbType.Int).Value = ComboTypOlej.SelectedIndex;
                        SQLCommand.Parameters.Add("@TypTkaniny", SqlDbType.Int).Value = TxtTypTkaniny.SelectedIndex;
                        SQLCommand.Parameters.Add("@ZamBuforowe", SqlDbType.Bit).Value = false;
                        SQLCommand.Parameters.Add("@Opis_wyrobu", SqlDbType.NVarChar).Value = TxtOpisWyrobu.Text;
                        if(GetIntOr(TxtWysokosc) == 0)
                           SQLCommand.Parameters.Add("@ZmianaH", SqlDbType.Int).Value = DBNull.Value;
                        else
                           SQLCommand.Parameters.Add("@ZmianaH", SqlDbType.Int).Value = GetIntOr(TxtWysokosc);

                        SQLCommand.Parameters.Add("@IdEOrder", SqlDbType.NVarChar).Value = TxtEOrderId.Text;
    
                        SQLCommand.ExecuteNonQuery();

                        if(TxtNrZam.Text.Right(3) == "/01")
                        {
                            var bumpUp = new SqlCommand();
                            bumpUp.CommandType = CommandType.Text;
                            bumpUp.Connection = connection;
                            bumpUp.CommandText = "UPDATE dbo.Wartosci_Pomocnicze SET Wartość = Wartość + 1 WHERE Nazwa = 'IndeksZamowienia'";
                            bumpUp.ExecuteNonQuery();
                        }

                        var commandKlient = new SqlCommand();
                        commandKlient.CommandType = CommandType.Text;
                        commandKlient.Connection = connection;
                        commandKlient.CommandText = "UPDATE dbo.Klienci SET Zamówiono = Zamówiono + 1 WHERE ID = @id;";
                        commandKlient.Parameters.Add("@id", SqlDbType.Int).Value = SelectedPodklient.ID;

                        commandKlient.ExecuteNonQuery();
                        // object? rtnValue;

                        if(!string.IsNullOrEmpty(TxtAdresDostawy.Text))
                        {
                        /*  try
                            {
                                var kAdresSql = new SqlCommand();
                                kAdresSql.CommandType = CommandType.Text;
                                kAdresSql.Connection = connection;
                                kAdresSql.CommandText = "SELECT ID FROM dbo.Klienci_Adresy WHERE KlientID = " + SelectedPodklient.ID + " AND AdresDostawy = @param1;";
                                kAdresSql.Parameters.Add("@param1",SqlDbType.NVarChar).Value = TxtAdresDostawy.Text;
                                rtnValue = kAdresSql.ExecuteScalar();
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.Message);
                                throw(e);
                            } */

                            bool bAddAdres = await AjramStaticData.IsQueryEmpty("SELECT ID FROM dbo.Klienci_Adresy WHERE KlientID = " + SelectedPodklient.ID + " AND AdresDostawy = @param1;", TxtAdresDostawy.Text);

                            if(bAddAdres)
                            {
                                var cmdAdres = new SqlCommand();
                                cmdAdres.CommandType = CommandType.Text;
                                cmdAdres.Connection = connection;
                                cmdAdres.CommandText = "INSERT INTO dbo.Klienci_Adresy (KlientID, AdresDostawy) VALUES (@KlientID, @AdresDostawy);";
                                cmdAdres.Parameters.Add("@AdresDostawy", SqlDbType.NVarChar).Value = TxtAdresDostawy.Text;
                                cmdAdres.Parameters.Add("@KlientID", SqlDbType.Int).Value = SelectedPodklient.ID;
                                cmdAdres.ExecuteNonQuery();
                                cbAdresy.Add(TxtAdresDostawy.Text);
                                AjramStaticData.AddAdresFirmy(TxtAdresDostawy.Text, SelectedPodklient.ID);
                            }
                        }
                        
                        if(!string.IsNullOrEmpty(TxtTkanina.Text))
                        {
                            if(TxtTkanina.Text.Length > 2)
                            {
                                if(TxtTypTkaniny.SelectedIndex != 0)
                                {
                                    decimal d = await EvaluateAmountOfTextureNeeded(TxtComboModelKrzeslo.Text, TxtTypProduktu.Text, GetIntOr(TxtIlosc), TxtTypTkaniny.SelectedIndex);
                                    Debug.WriteLine("Tkanina: " + d.ToString());
                                    var tkInsert = new SqlCommand();
                                    tkInsert.CommandType = CommandType.Text;
                                    tkInsert.Connection = connection;
                                    tkInsert.CommandText = "INSERT INTO dbo.Zam_Tkaniny (Tkanina, Dostawca, ZamID, Ilosc, Niestandardowe) VALUES (@tk, @dostawca, @zamID, @ilosc, @niestandardowe);";
                                    tkInsert.Parameters.Add("@tk", SqlDbType.NVarChar).Value = TxtTkanina.Text;
                                    tkInsert.Parameters.Add("@dostawca", SqlDbType.NVarChar).Value = TxtComboDostawcaTkaniny.Text;
                                    tkInsert.Parameters.Add("@zamID", SqlDbType.NVarChar).Value = TxtNrZam.Text;
                                    tkInsert.Parameters.Add("@ilosc", SqlDbType.Decimal).Value = d;
                                    tkInsert.Parameters.Add("@niestandardowe", SqlDbType.Bit).Value = CheckPowierzonaNiestandardowa.IsChecked;

                                    tkInsert.ExecuteNonQuery();
                                }
                            }
                        }

                        if((TxtEOrderId.Text ?? "") != "")
                        {
                            bProceedWithEOrders = await TryToCompleteEOrder(TxtEOrderId.Text, TxtNrZam.Text);
                            if(bProceedWithEOrders)
                            {
                                ReduceOrderCountProperly();
                            }
                            else
                            {
                                MessageBox.Show("Nie udało się usunać zamówienia z internetowej bazy danych.", "Błąd");
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                    connection.Close();
                    ClearAllData();

                    if(bProceedWithEOrders)
                        GoForNextOrder();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        public void ReduceOrderCountProperly()
        {
            TotalOrderCount--;
            if(SelectedEKlient != null)
            {
                if(SelectedEKlient.iCount == 1)
                {
                    cbKlienciByOrders.Remove(SelectedEKlient);
                    SelectedEKlient = null;
                }
                else
                    SelectedEKlient.iCount--;
            }
        }

        public async Task<bool> TryToCompleteEOrder(string eOrder, string ajrOrder)
        {
            var os = OrdersStore.Instance.Orders.FirstOrDefault(f => f.Id == eOrder);
            if(os == null)
                return false;

            return await OrdersStore.Instance.CompleteOrderAsync(os, ajrOrder);
        }
        public bool IsOrderNumberOutdated()
        {
            if(!AjramStaticData.IsQueryEmpty("SELECT ID FROM dbo.Zamowienia WHERE Nr_zamówienia = @param1;", TxtNrZam.Text).Result)
                return true;

            return !AjramStaticData.IsQueryEmpty("SELECT ID FROM dbo.Zamowienia_Historia WHERE Nr_zamówienia = @param1;", TxtNrZam.Text).Result;
        }
        public bool CheckIfComboBoxIsEmpty(ComboBox cb, string sError = "")
        {
            if(cb.SelectedIndex == -1)
            {
                if(sError != "")
                    MessageBox.Show("Pole <<" + sError + ">> musi zostać wypełnione.");
                return true;
            }
            return false;
        }
        public bool CheckIfTextBoxIsEmpty(TextBox tb, string sError = "")
        {
            if(string.IsNullOrEmpty(tb.Text))
            {
                if(sError != "")
                    MessageBox.Show("Pole <<" + sError + ">> musi zostać wypełnione.");
                return true;
            }
            return false;
        }

        public int GetIntOr(TextBox tb, int orVal = 0)
        {
            if(int.TryParse(tb.Text, out int iIlosc))
                return iIlosc;

            return orVal;
        }
        public bool PassedAjramChecks()
        {
            int iIlosc = GetIntOr(TxtIlosc);
            if(iIlosc <= 0 || iIlosc >= 30000)
            {
                MessageBox.Show("Nieprawidłowe dane w polu ilość."); 
                return false;
            }
            if(CheckIfComboBoxIsEmpty(TxtComboWaluta, "Waluta")) return false;
            if(CheckIfTextBoxIsEmpty(TxtNrZam, "Nr zamówienia")) return false;
            if(CheckIfComboBoxIsEmpty(TxtComboDrewno, "Gatunek drewna")) return false;
            if(CheckIfComboBoxIsEmpty(ComboTypOlej, "Olejowanie/lakierowanie")) return false;
            if(CheckIfComboBoxIsEmpty(TxtTypTkaniny, "Typ tkaniny")) return false;
            if(TxtTypProduktu.Text == "Stół")
            {
                if(CheckIfComboBoxIsEmpty(TxtComboModelStol, "Model")) return false;
            }
            else
            {
                if(CheckIfComboBoxIsEmpty(TxtComboModelKrzeslo, "Model")) return false;
            }
            int iRabat = GetIntOr(TxtRabat);
            if(iRabat > 95)
            {
                MessageBox.Show("Maksymalny rabat wynosi 95%."); 
                return false;
            }
            if(SelectedPodklient == null)
            {
                MessageBox.Show("Pole podklient jest puste."); 
                return false;
            }
            int iWartosc = GetIntOr(TxtWartosc);
            if(iWartosc < 0)
            {
                MessageBox.Show("Nieprawidłowe dane w polu Wartość."); 
                return false;
            }
            if(TxtTypTkaniny.Text == "Brak" && !string.IsNullOrEmpty(TxtTkanina.Text))
            {
                MessageBoxResult dialogResult = MessageBox.Show("Typ tkaniny określono jako brak ale w polu tkaniny znajduje się tekst.\n\nCzy chcesz kontynuować?", "Brak tkaniny?", MessageBoxButton.YesNo);
                if(dialogResult == MessageBoxResult.Yes)
                {
                    //do something
                }
                else if (dialogResult == MessageBoxResult.No)
                {
                    return false;
                }
            }
            return true;
        }
        public bool FailedComboBoxCheck(ComboBox cb, string fError, ObservableCollection<string> ob)
        {
            if(cb.Text == null || cb.Text == "")
                return false;

            if(ob.Contains(cb.Text))
                return false;

            MessageBox.Show("Nieprawidłowe dane dla pola: <" + fError + ">.");
            return true;
        }

        public bool FailedLengthCheck(TextBox textbox, int maxlength, string fError)
        {
            if(textbox.Text != null || textbox.Text != "")
            {
                string s = textbox.Text.Replace("'", "-");
                s = s.Replace("\n", String.Empty);
                s = s.Replace("\r", String.Empty);
                s = s.Replace("\t", String.Empty);
                textbox.Text = s.Trim();
            }

            if(maxlength == -1)
                return false;

            if(textbox.Text.Length < maxlength)
                return false;

            MessageBox.Show("Tekst pola: <" + fError + "> jest za długi (max: " + maxlength.ToString() + ").");
            return true;
        }
        public bool PassedLength()
        {
            if(FailedLengthCheck(TxtDataWysyłki, 255, "Data wysyłki")) return false;
            if(FailedLengthCheck(TxtNrZamKlient, 255, "Nr zamówienia klienta")) return false;
            if(FailedLengthCheck(TxtAdresDostawy, 255, "AdresDostawy")) return false;
            if(FailedLengthCheck(TxtKolor, 255, "Kolor")) return false;
            if(FailedLengthCheck(TxtTkanina, 255, "ZmianyIDodatki")) return false;
            if(FailedLengthCheck(TxtBrendowanie, 255, "Brendowanie")) return false;
            if(FailedLengthCheck(TxtOpisWyrobu, -1, "x")) return false;
            if(FailedLengthCheck(TxtZmianyIDodatki, -1, "x")) return false;
            if(FailedLengthCheck(TxtUwagi, -1, "x")) return false;

            return true;
        }
        public bool PassedComboChecks()
        {
            if(FailedComboBoxCheck(TxtComboDostawcaTkaniny, "Dostawca tkaniny", cbDostawcaTkaniny)) return false;
            if(TxtTypProduktu.Text == "Stół")
            {
                if(FailedComboBoxCheck(TxtComboModelStol, "Model", cbStoly)) return false;
            }
            else
            {
                if(FailedComboBoxCheck(TxtComboModelKrzeslo, "Model", cbKrzesla)) return false;
            }

            return true;
        }
        private void AddNewOrderNumber_Click(object sender, RoutedEventArgs e)
        {
            AddNewOrder();
        }
        public async Task AddNewOrder()
        {
            if (!ConnectionService.Instance.IsConnected)
                throw new InvalidOperationException("Local database is not connected.");

            try
            {
                using (var connection = ConnectionService.Instance.GetConnection())
                {
                    await connection.OpenAsync();

                    int storedYear = 0;
                    int serverYear = 0;
                    int indeksZam = 0;
                    try
                    {
                        var SQLCommand = new SqlCommand();
                        SQLCommand.CommandType = CommandType.Text;
                        SQLCommand.Connection = connection;
                        SQLCommand.CommandText = "SELECT Wartość FROM dbo.Wartosci_Pomocnicze WHERE Nazwa = 'RokIndeksZamowienia';";
                        storedYear = (int) SQLCommand.ExecuteScalar();
                        SQLCommand.CommandText = "SELECT YEAR(GetDate()) as CurrentTime;";
                        serverYear = (int) SQLCommand.ExecuteScalar();

                        
                        if(serverYear != storedYear)
                        {
                            SQLCommand.CommandText = "UPDATE dbo.Wartosci_Pomocnicze SET Wartość = " + serverYear + " WHERE Nazwa = 'RokIndeksZamowienia'";
                            SQLCommand.ExecuteNonQuery();
                            SQLCommand.CommandText = "UPDATE dbo.Wartosci_Pomocnicze SET Wartość = 1 WHERE Nazwa = 'IndeksZamowienia'";
                            SQLCommand.ExecuteNonQuery();
                            indeksZam = 1;
                        }
                        else
                        {
                            SQLCommand.CommandText = "SELECT Wartość FROM dbo.Wartosci_Pomocnicze WHERE Nazwa = 'IndeksZamowienia';";
                            indeksZam = (int) SQLCommand.ExecuteScalar();
                        }

                        TxtNrZam.Text = indeksZam.ToString() + "/" + serverYear.ToString().Right(2) + "/01";
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"System Error: {e.Message}");
                    }


                    connection.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"System Error: {e.Message}");
            }
        }
        private void AddLastClientNumber_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedPodklient == null)
                MessageBox.Show("Nie zaznaczono podklienta.");
            else
                AddLastValidOrder(SelectedPodklient.ID);
        }
        public async Task<Decimal> EvaluateAmountOfTextureNeeded(string sModel, string sTyp, int sztuki, int iTypTkaniny)
        {
            if(sTyp != "Krzesło")
                return 0;

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            Decimal totalmeters = 0;
            try
            {
                using (var connection = ConnectionService.Instance.GetConnection())
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("SELECT Tkanina1, Tkanina2, Tkanina3, Tkanina4, Skora FROM dbo.Modele_Krzesla_Drewniane WHERE Model = @param1;", connection);
                    cmd.Parameters.Add("@param1", SqlDbType.NVarChar).Value = sModel;
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds, "DaneTkaniny");

                    foreach (DataRow row in ds.Tables["DaneTkaniny"].Rows)
                    {
                        totalmeters = EvaluteTextureNeeded(iTypTkaniny, sztuki, (Decimal) row["Tkanina1"], (Decimal) row["Tkanina2"], (Decimal) row["Tkanina3"], (Decimal) row["Tkanina4"], (Decimal) row["Skora"]);
                        break;
                    }
                    connection.Close(); 
                }
            }
            catch(Exception e){
                Debug.WriteLine(e.ToString());
            }
            return totalmeters;
        }

        public int GetOptimalOptionForSzablon(int iToDo, decimal tk1, decimal tk2, decimal tk3, decimal tk4)
        {
            int goofs = 0;
            decimal dHelp = 0;
            decimal dPerOne = -1;
            if(tk4 != 0 && iToDo >= 4)
            {
                goofs = 4;
                if(iToDo == 4)
                    return 4;
                else if(iToDo == 5)
                    dPerOne = (tk4 + tk1) / 5;
                else if(iToDo == 6)
                {
                    if(tk2 != 0)
                        dPerOne = (tk4 + tk2) / 6;
                    else
                        dPerOne = (tk4 + 2 * tk1) / 6;
                }
                else
                    dPerOne = tk4/ 4;
            }
            if(tk3 != 0 && iToDo >= 3)
            {
                if(iToDo == 3)
                    return 3;
                
                if(iToDo == 4)
                    dHelp = (tk3 + tk1) / 4;
                else if(iToDo == 5)
                {
                    if(tk2 != 0)
                        dHelp = (tk3 + tk2) / 5;
                    else
                        dHelp = (tk3 + 2 * tk1) / 5;
                }
                else
                    dHelp = tk3 / 3;

                if(dPerOne == -1 || dHelp < dPerOne)
                {
                    goofs = 3;
                    dPerOne = dHelp;
                }
            }
            if(tk2 != 0 && iToDo >= 2)
            {
                if(iToDo == 2)
                    return 2;

                if(iToDo == 5)
                    dHelp = (2 * tk2 + tk1) / 5;
                else
                    dHelp = tk2 / 2;

                if(dPerOne == -1 || dHelp < dPerOne)
                {
                    goofs = 2;
                    dPerOne = dHelp;
                }
            }
            if(dPerOne == -1 || tk1 < dPerOne)
                goofs = 1;

            return goofs;
        }
        public int[] GenerateSzablonForTkaninaNeeded(int iSztuki, decimal tk1, decimal tk2, decimal tk3, decimal tk4)
        {
            int[] rArray = new int[4]{0,0,0,0};
            int iToDo = iSztuki;
            int iOptimal = 0;
            int iTimes = 0;
            int h = 0;
            while(iToDo > 0 && h < 100)
            {
                iOptimal = GetOptimalOptionForSzablon(iToDo, tk1, tk2, tk3, tk4);
                if(iOptimal < 1)
                {
                    MessageBox.Show("Error 943/34/1/223/44.");
                    return rArray;
                }
                else
                {
                    if(iOptimal == 1)
                        iTimes = iToDo;
                    else
                    {
                        iTimes = (iToDo - 4) / iOptimal;
                        if(iTimes < 1)
                            iTimes = 1;

                    }
                    Debug.WriteLine("Log: " + iToDo.ToString() + ", " + iOptimal.ToString() + ", " + iTimes.ToString());
                    iToDo = iToDo - iOptimal * iTimes;
                    rArray[iOptimal - 1] = rArray[iOptimal - 1] + iTimes; 
                }
                h++;
            }
            return rArray;
        }
        public Decimal EvaluteTextureNeeded(int iTypTkaniny, int sztuki, Decimal t1, Decimal t2, Decimal t3, Decimal t4, Decimal skora)
        {
            Decimal rtnvalue = 0;

            if(iTypTkaniny == 1)
            {
                int[] rSzablon = GenerateSzablonForTkaninaNeeded(sztuki, t1, t2, t3, t4);
                rtnvalue = t1 * rSzablon[0] + t2 * rSzablon[1] + t3 * rSzablon[2] + t4 * rSzablon[3];
            }
            else
            {
                rtnvalue = skora * sztuki;
            }
            rtnvalue = Math.Ceiling(rtnvalue * 10) / 10;
            return rtnvalue;
        }
        public async Task AddLastValidOrder(int podklientid)
        {
            if (!ConnectionService.Instance.IsConnected)
                throw new InvalidOperationException("Local database is not connected.");

            DataSet ds = AjramStaticData.GetDataFromDBQuery("SELECT Nr_zamówienia FROM dbo.Zamowienia WHERE Indeks_Klient = " + podklientid + " OR PodindeksKlient = " + podklientid + ";");
            if(ds != null)
            {
                if(ds.Tables["MyQuery"] != null)
                {   
                    int iyear = 0;
                    int iorder = 0;
                    foreach (DataRow row in ds.Tables["MyQuery"].Rows)
                    {
                        string[] s = ((string) row["Nr_zamówienia"]).Split("/");
                        int.TryParse(s[1], out int cyear);
                        int.TryParse(s[0], out int corder);

                        if((cyear > iyear) || (cyear == iyear && corder > iorder))
                        {
                            iyear = cyear;
                            iorder = corder;
                        }
                    }

                    if(iyear != 0)
                    {
                        int lLength = 3 + iorder.ToString().Length; 
                        string sSql = "SELECT Nr_zamówienia FROM dbo.Zamowienia WHERE LEFT(Nr_zamówienia, " + lLength.ToString() + ") = '" + iorder.ToString() + "/" + iyear.ToString() + "';";
                        ds = AjramStaticData.GetDataFromDBQuery(sSql);   
                        if(ds.Tables["MyQuery"] != null)
                        {   
                            int greatest = 0;
                            foreach (DataRow row in ds.Tables["MyQuery"].Rows)
                            {
                                string[] s = ((string) row["Nr_zamówienia"]).Split("/");
                                int.TryParse(s[2], out int horder);

                                if(horder > greatest)
                                    greatest = horder;
                            }
                            if(greatest > 0)
                            {
                                greatest = greatest + 1;
                                TxtNrZam.Text = iorder + "/" + iyear.ToString().Right(2) + "/" + string.Format("{0:00}", greatest);
                                return;
                            }
                        }
                    }
                }
            }
            TxtNrZam.Text = "";
            MessageBox.Show("Nie udało się wygenerować podpunktu dla tego klienta.");
        }
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
     
     
     /*       if (DropdownMenu.Visibility == Visibility.Visible &&
                !DropdownMenu.IsMouseOver && !TxtAvatarLetter.IsMouseOver)
            {
                DropdownMenu.Visibility = Visibility.Collapsed;
            } */
        }
        private void CBTypProduktu_DropDownClosed(object sender, EventArgs e) {
            ComboBox cmb = sender as ComboBox;
            bShowKrzesla = cmb.SelectedIndex == 0;
        }

        public bool IsTextValidInteger(string s, int ml = 9)
        {
            if(s.Length > ml)
                return false;

            return int.TryParse(s, out int n);
        }

        public bool IsTextValidDecimal(string s, int dp, int ml = 10)
        {
            if(s.Length > ml)
                return false;

            if(float.TryParse(s, out float f))
            {
                if(s.Split(",").Count() > 1)
                {
                    if(s.Split(",")[1].Length > dp)
                        return false;
                }
                return true;
            }
            return false;
        }

        private void PreviewTextInteger4InputHandler(Object sender, TextCompositionEventArgs e) 
        { 
            TextBox t = sender as TextBox;
            e.Handled = !IsTextValidInteger(t.Text + e.Text, 4); 
        } 
        private void IntegerPasting4Handler(object sender, DataObjectPastingEventArgs e) 
        { 
            if (e.DataObject.GetDataPresent(typeof(String))) 
            { 
                TextBox t = sender as TextBox;
                String text = (String)e.DataObject.GetData(typeof(String)); 
                if (!IsTextValidInteger(t.Text + text, 4)) e.CancelCommand(); 
            } 
            else e.CancelCommand(); 
        }
        private void PreviewTextInteger2InputHandler(Object sender, TextCompositionEventArgs e) 
        { 
            TextBox t = sender as TextBox;
            e.Handled = !IsTextValidInteger(t.Text + e.Text, 2); 
        } 
        private void IntegerPasting2Handler(object sender, DataObjectPastingEventArgs e) 
        { 
            if (e.DataObject.GetDataPresent(typeof(String))) 
            { 
                TextBox t = sender as TextBox;
                String text = (String)e.DataObject.GetData(typeof(String)); 
                if (!IsTextValidInteger(t.Text + text, 2)) e.CancelCommand(); 
            } 
            else e.CancelCommand(); 
        }
        private void PreviewTextDecimal2InputHandler(Object sender, TextCompositionEventArgs e) 
        { 
            TextBox t = sender as TextBox;
            e.Handled = !IsTextValidDecimal(t.Text + e.Text, 2, 10); 
        } 
        private void DecimalPasting2Handler(object sender, DataObjectPastingEventArgs e) 
        { 
            if (e.DataObject.GetDataPresent(typeof(String))) 
            { 
                TextBox t = sender as TextBox;
                String text = (String)e.DataObject.GetData(typeof(String)); 
                if (!IsTextValidDecimal(t.Text + text, 2, 10)) e.CancelCommand(); 
            } 
            else e.CancelCommand(); 
        }
        private void CBTypProduktu_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cmb = sender as ComboBox;
            bShowKrzesla = cmb.SelectedIndex == 0;
        }
        private bool comboadreshandle = true;
        private void HandleComboAdres()
        {
            if(TxtComboAdres.SelectedItem != null)
            {
                TxtAdresDostawy.Text = TxtComboAdres.SelectedItem.ToString();
                TxtComboAdres.SelectedItem = null;
            }
        }
        private void ComboAdres_DropDownClosed(object sender, EventArgs e) {
            if(comboadreshandle)
                HandleComboAdres();
                
            comboadreshandle = true; 
        }
        private void ComboAdres_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cmb = sender as ComboBox;
            comboadreshandle = !cmb.IsDropDownOpen;
            HandleComboAdres();
        }
        private bool combokolorhandle = true;
        private void HandleComboKolor()
        {
            if(TxtComboKolor.SelectedItem != null)
            {
                TxtKolor.Text = TxtComboKolor.SelectedItem.ToString();
                TxtComboKolor.SelectedItem = null;
                TxtComboKolor.Text = "";
            }
        }
        private void ComboKolor_DropDownClosed(object sender, EventArgs e) {
            if(combokolorhandle)
                HandleComboKolor();
                
            combokolorhandle = true; 
        }
        private void ComboKolor_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cmb = sender as ComboBox;
            combokolorhandle = !cmb.IsDropDownOpen;
            HandleComboKolor();
        }
        private void ComboKolor_LostFocus(object sender, RoutedEventArgs e)
        {
            TxtComboKolor.SelectedItem = null;
            TxtComboKolor.Text = "";
        }


        private void ClearAllData()
        {
            TxtNrZam.Text = null;
            TxtEOrderId.Text = null;
            CurrentEOrderId = "";
            TxtComboDrewno.SelectedItem  = null;
            TxtComboModelKrzeslo.SelectedItem = null;
            TxtComboModelStol.SelectedItem = null;
            TxtIlosc.Text = null;
            TxtKolor.Text = null;
            TxtTkanina.Text = null;
            TxtComboDostawcaTkaniny.SelectedItem = null;
            TxtUwagi.Text = null;
            TxtZmianyIDodatki.Text = null;
            TxtOpisWyrobu.Text = null;
            TxtTypPakowania.SelectedIndex = 0;
            TxtWysokosc.Text = null;
            TxtComboSlizgacz.SelectedItem = null;
            TxtComboBrylanciki.SelectedItem = null;
            TxtComboKolatka.SelectedItem = null;
            TxtComboWaluta.SelectedItem = "PLN";
            TxtComboKolka.SelectedItem = null;
            TxtComboBrendowanie.SelectedItem = null;
            TxtComboEtykieta.SelectedItem = null;
            TxtComboGwozdziki.SelectedItem = null;
            TxtWartosc.Text = null;
            TxtBrendowanie.Text = null;
            ComboTypOlej.SelectedItem = null;
            TxtRabat.Text = "0";
            TxtTypTkaniny.SelectedItem = null;
            CheckPowierzonaNiestandardowa.IsChecked = false;
            TxtNotkaKlienta.Text = null;
        }
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public class KlientWithOrderCount : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Firma {get; set;} = "";
        private int _count = 0;
        public int iCount 
        {
            get => _count;
            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged(nameof(iCount));
                }
            }
        }
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}