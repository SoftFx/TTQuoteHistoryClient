using Microsoft.Windows.Controls;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using TickTrader.BusinessObjects;
using TickTrader.Common.Business;
using TTQuoteHistoryCache;
using TTQuoteHistoryClient;
using MessageBox = System.Windows.MessageBox;

namespace TTQuoteHistoryCacheSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            EnableQuoteHistoryControls(false);

            pickerTimestamp.Format = DateTimeFormat.Custom;
            pickerTimestamp.FormatString = "yyyy.MM.dd HH:mm:ss.fff";
            pickerTimestamp.Value = DateTime.UtcNow;
            textCount.Text = "100";
        }

        QuoteHistoryClient _historyClient;
        HistoryCacheManager _historyCacheManager;

        private void EnableQuoteHistoryControls(bool enabled)
        {
            comboboxSymbol.IsEnabled = enabled;
            comboboxPeriodicity.IsEnabled = enabled;
            comboboxPriceType.IsEnabled = enabled;
            pickerTimestamp.IsEnabled = enabled;
            textCount.IsEnabled = enabled;
            request.IsEnabled = enabled;
            textHistory.IsEnabled = enabled;
            textIntervals.IsEnabled = enabled;
        }

        private void InitializeQuoteHistory()
        {
            DisposeQuoteHistory();

            _historyClient = new QuoteHistoryClient();
            _historyClient.Connected += HistoryClientOnConnected;
            _historyClient.ConnectError += HistoryClientOnConnectError;
            _historyClient.Disconnected += HistoryClientOnDisconnected;
            _historyClient.Connect(textAddress.Text);

            _historyCacheManager = new HistoryCacheManager(_historyClient);
            _historyCacheManager.Initialize();

            var symbols = _historyCacheManager.GetSymbols();
            foreach (var symbol in symbols)
                comboboxSymbol.Items.Add(symbol);
            comboboxSymbol.SelectedIndex = 0;

            var periodicities = _historyCacheManager.GetPeriodicities();
            foreach (var periodicity in periodicities)
                comboboxPeriodicity.Items.Add(periodicity.ToString());
            comboboxPeriodicity.SelectedIndex = 0;

            comboboxPriceType.Items.Add("Bid");
            comboboxPriceType.Items.Add("Ask");
            comboboxPriceType.Items.Add("Ticks");
            comboboxPriceType.Items.Add("Level2");
            comboboxPriceType.SelectedIndex = 0;
        }

        private void HistoryClientOnConnected(QuoteHistoryClient client)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                EnableQuoteHistoryControls(true);
                connect.Content = "Disconnect";
            }));
        }

        private void HistoryClientOnConnectError(QuoteHistoryClient client)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(DisposeQuoteHistory));
        }

        private void HistoryClientOnDisconnected(QuoteHistoryClient client)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                EnableQuoteHistoryControls(false);
                connect.Content = "Connect";
            }));
        }

        private void DisposeQuoteHistory()
        {
            if (_historyCacheManager != null)
            {
                _historyCacheManager.Close();
                _historyCacheManager = null;
            }
            if (_historyClient != null)
            {
                _historyClient.Connected -= HistoryClientOnConnected;
                _historyClient.ConnectError -= HistoryClientOnConnectError;
                _historyClient.Disconnected -= HistoryClientOnDisconnected;

                _historyClient.Dispose();
                _historyClient = null;
            }

            comboboxSymbol.Items.Clear();
            comboboxSymbol.SelectedIndex = -1;
            comboboxPeriodicity.Items.Clear();
            comboboxPeriodicity.SelectedIndex = -1;
            comboboxPriceType.Items.Clear();
            comboboxPriceType.SelectedIndex = -1;

            textHistory.Text = "";
            textIntervals.Text = "";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DisposeQuoteHistory();
            Application.Current.Shutdown(0);
        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((connect.Content as string) == "Connect")
                {
                    InitializeQuoteHistory();
                }
                else
                {
                    DisposeQuoteHistory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection error");
            }
        }

        private void request_Click(object sender, RoutedEventArgs e)
        {
            textHistory.Text = "";
            textIntervals.Text = "";

            if (!pickerTimestamp.Value.HasValue)
                return;

            var symbol = comboboxSymbol.SelectedItem as string;
            var periodicity = comboboxPeriodicity.SelectedItem as string;
            var priceType = comboboxPriceType.SelectedItem as string;
            var timestamp = pickerTimestamp.Value.Value;
            var count = int.Parse(textCount.Text);

            var result = new StringBuilder();
            if ((priceType == "Bid") || (priceType == "Ask"))
            {
                var bars = _historyCacheManager.GetBarsHistory(symbol, Periodicity.Parse(periodicity), (priceType == "Bid") ? FxPriceType.Bid : FxPriceType.Ask, timestamp, count);
                foreach (var bar in bars)
                    result.AppendLine($"{bar.Time:yyyy.MM.dd HH:mm:ss.fff}: {bar.Open:0.#####}, {bar.Hi:0.#####}, {bar.Low:0.#####}, {bar.Close:0.#####}, {bar.Volume:0.#####}");
                textIntervals.Text = _historyCacheManager.DumpBarsIntervals(symbol, Periodicity.Parse(periodicity), (priceType == "Bid") ? FxPriceType.Bid : FxPriceType.Ask);
            }
            else
            {
                var ticks = _historyCacheManager.GetTicksHistory(symbol, (priceType == "Level2"), timestamp, count);
                foreach (var tick in ticks)
                {
                    result.Append($"{tick.Time:yyyy.MM.dd HH:mm:ss.fff}: ");
                    if (tick.HasBid)
                        result.Append($"Best bid: {tick.BestBid.Price:0.#####} {tick.BestBid.Volume:0.#####} ");
                    if (tick.HasAsk)
                        result.Append($"Best ask: {tick.BestAsk.Price:0.#####} {tick.BestAsk.Volume:0.#####} ");
                    result.Append($"Bids: {tick.Level2.BidCount} ");
                    result.Append($"Asks: {tick.Level2.AskCount} ");
                    result.AppendLine();
                }
                textIntervals.Text = _historyCacheManager.DumpTicksIntervals(symbol, (priceType == "Level2"));
            }
            textHistory.Text = result.ToString();
        }
    }
}
