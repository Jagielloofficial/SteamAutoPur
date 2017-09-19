using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Linq;
using SteamAutoPurchase.Utils;

namespace SteamAutoPurchase
{
    [DataContract(Name = "Items", Namespace = "")]
    public class ItemPage
    {
        public SteamAuth.UserLogin Login { get; set; }

        public string DescriptionFilter { get; private set; }

        [DataMember(Name = "Name", Order = 0)]
        private string _itemName;
        [DataMember(Name = "Price", Order = 1)]
        private int _rigthPrice;
        [DataMember(Name = "Description", Order = 2, EmitDefaultValue = false)]
        private string _rigthDescription;
        [DataMember(Name = "Socket", Order = 3, EmitDefaultValue = false)]
        private string _rigthSocket;

        private List<DescriptionPosition> _descriptions;

        public List<Item> Items;

        private string _stringHttp;

        private string _url;

        private const int Wait = 12;


        internal ItemPage(SteamAuth.UserLogin login, string itemName, int rigthPrice, string rigthDescription = null, string rigthSocket = null)
        {
            Login = login;
            _itemName = itemName.ChangeItemNameForSite();
            _rigthPrice = rigthPrice;
            _rigthDescription = rigthDescription;
            _rigthSocket = rigthSocket;

            Items = new List<Item>();

            DescriptionFilter = _rigthSocket is null && _rigthDescription != null
                ? $@"{_rigthDescription.ChangeDescriptionForSite()}+NOT+locked"
                : _rigthSocket != null
                    ? $"{_rigthSocket.ChangeDescriptionForSite()}"
                    : throw new Exception("��������� ������ ��� ��������");
        }


        internal async System.Threading.Tasks.Task LoadMarketplacePageAsync()
        {
            var client = new System.Net.Http.HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36");

            Console.WriteLine(_itemName + "   " + DescriptionFilter);

            try
            {
                _url = $@"http://steamcommunity.com/market/listings/570/{_itemName}/render/?filter={DescriptionFilter}?query=&country=BY&language=english&currency=1";
                var response = await client.GetAsync(_url);
                response.EnsureSuccessStatusCode();

                _stringHttp = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                if (e.Message == "��� ��������� ������ �� ��������� �� �������� ����������: 429 (Too Many Requests).")
                {
                    Console.WriteLine(@"Error 429, Sleep 60 seconds");
                }
                else
                {
                    Console.WriteLine(@"Unknown Error, Sleep 60 seconds");

                    Console.WriteLine(e.Message);
                }
                Thread.Sleep(Wait * 2);
            }

            if (string.IsNullOrEmpty(_stringHttp)) return;

            try
            {
                GetDescription();

                GetItems();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Thread.Sleep(Wait * 1000);
        }

        private void GetDescription()
        {
            const string stringStartDescription = "\"descriptions\":[{\"type\":\"html\",\"value\":";
            const string stringEndDescription = "}],";

            _descriptions = new List<DescriptionPosition>();

            var startDescription = 0;
            var endDescription = 0;

            while (true)
            {
                startDescription = _stringHttp.IndexOf(stringStartDescription, startDescription + 1,
                    StringComparison.Ordinal);

                if (startDescription == -1) break;

                endDescription = _stringHttp.IndexOf(stringEndDescription, endDescription + 1, StringComparison.Ordinal);

                var description = new DescriptionPosition
                {
                    StartDescriptionTagPosition = startDescription,
                    EndDescriptionTagPosition = endDescription
                };

                _descriptions.Add(description);
            }
        }

        private void GetItems()
        {
            var ids = ParseIds();

            var prices = ParsePrices();

            if (_rigthPrice != 0 && prices.All(price => price.SubTotal > _rigthPrice))
            {
                return;
            }

            for (var index = 0; index < _descriptions.Count; index++)
            {
                var description = _descriptions[index];

                var length = description.EndDescriptionTagPosition - description.StartDescriptionTagPosition;

                var substr = _stringHttp.Substring(description.StartDescriptionTagPosition, length);

                var item = FillItem(substr, length, index);

                item.Id = ids[index];
                item.Price = prices[index];

                Items.Add(item);
                
                if (_rigthDescription != null)
                {
                    foreach (var desc in item.Descriptions)
                    {
                        if (item.Price.SubTotal <= _rigthPrice && desc == _rigthDescription)
                        {
                            BuyItem(item);
                        }
                    }
                }

                if (_rigthSocket != null)
                {
                    foreach (var socket in item.Sockets)
                    {
                        if (item.Price.SubTotal <= _rigthPrice && socket == _rigthSocket)
                        {
                            BuyItem(item);
                        }
                    }
                }
            }
        }

        private void BuyItem(Item item)
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("steamMachineAuth" + Login.Session.SteamID, Login.Session.WebCookie, "/", ".steamcommunity.com"));
            //cookieContainer.Add(new Cookie("steamRememberLogin", "", "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("recentlyVisitedAppHubs", "570", "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("strInventoryLastContext", "570_2", "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("sessionid", Login.Session.SessionID, "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("steamLogin", Login.Session.SteamLogin, "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("steamLoginSecure", Login.Session.SteamLoginSecure, "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("webTradeEligibility",
                "%7B%22allowed%22%3A1%2C%22allowed_at_time%22%3A0%2C%22steamguard_required_days%22%3A15%2C%22sales_this_year%22%3A1550%2C%22max_sales_per_year%22%3A-1%2C%22forms_requested%22%3A0%2C%22new_device_cooldown_days%22%3A7%2C%22time_checked%22%3A1504217756%7D",
                "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("timezoneOffset", "10800.0", "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("_ga", "GA1.2.1930146676.1500737117", "/", ".steamcommunity.com"));
            cookieContainer.Add(new Cookie("_gid", "GA1.2.180454504.1503201245", "/", ".steamcommunity.com"));

            var totalValue = item.Price.SubTotal - item.Price.Currency;

            var str = "sessionid=" + Login.Session.SessionID + $@"&currency=1&subtotal={item.Price.Currency}&fee={totalValue}&total={item.Price.SubTotal}&quantity=1"; 

            var url = "https://steamcommunity.com/market/buylisting/" + item.Id;
            var requast = (HttpWebRequest)WebRequest.Create(url);
            requast.Method = "POST";
            requast.Host = "steamcommunity.com";
            requast.ContentLength = str.Length;
            requast.Accept = "*/*";
            requast.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.125 YaBrowser/17.7.1.791 Yowser/2.5 Safari/537.36";
            requast.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            requast.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            requast.Referer = _url;
            requast.CookieContainer = cookieContainer;
            requast.KeepAlive = true;
            requast.ServicePoint.Expect100Continue = false;
            requast.Headers.Add("Accept-Language", "ru,en;q=0.8");

            Thread.Sleep(100); // �� ���� �������� - ������� ������

            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                using (var stream = new System.IO.StreamWriter(requast.GetRequestStream()))
                {
                    stream.Write(str);
                }

                Console.WriteLine(_itemName + " is bought");
            }
            catch (Exception e) when (e.Message == "��������� ������ ��������� ������: (502) ������������ ����.")
            {
                Console.WriteLine("502 ������������ ����");
            }
            catch (Exception e)
            {
                Console.WriteLine("BuyError:");
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private static Item FillItem(string substr, int length, int index)
        {
            return new Item
            {
                Index = index,
                Sockets = ParseSockets(length, substr),
                Descriptions = ParseStyles(substr)
            };
        }

        private List<Price> ParsePrices()
        {
            const string mask = "\\r\\n\\t\\t\\t\\t\\t\\t$";

            var index = 0;

            var prices = new List<Price>();

            while (true)
            {
                if (!_stringHttp.Contains(mask))
                { break; }

                try
                {
                    var price = new Price
                    {
                        SubTotal = GetPrice(),
                        Fee = GetPrice(),
                        Currency = GetPrice()
                    };

                    prices.Add(price);
                }
                catch (Exception e) when (e.Message == "End of Parsing prices")
                {
                    break;
                }
            }
            return prices;

            int GetPrice()
            {
                index = _stringHttp.IndexOf(mask, index, StringComparison.Ordinal) + mask.Length;
                var endIndex = _stringHttp.IndexOf(" ", index, StringComparison.Ordinal);

                var strPrice = _stringHttp.Substring(index, endIndex - index);

                int price;
                try
                {
                    strPrice = strPrice.Replace(".", "");

                    foreach (var str in strPrice)
                    {
                        if (str != '0' && char.IsDigit(str))
                        {
                            break;
                        }
                        strPrice = strPrice.Remove(0, 1);
                    }

                    price = int.Parse(strPrice);

                }
                catch (Exception)
                {
                    throw new Exception("End of Parsing prices");
                }

                return price;
            }
        }

        private List<string> ParseIds()
        {
            const string stringStartId = " listing_";
            const string stringEndId = "\\\" id=\\\"";

            var ids = new List<string>();

            var startId = 0;
            while (true)
            {
                startId = _stringHttp.IndexOf(stringStartId, startId, StringComparison.Ordinal);
                if (startId <= -1) break;
                startId += stringStartId.Length;

                var indexEndId = _stringHttp.IndexOf(stringEndId, startId, StringComparison.Ordinal);

                var id = _stringHttp.Substring(startId, indexEndId - startId);
                ids.Add(id);
            }
            return ids;
        }

        private static List<string> ParseStyles(string substr)
        {
            const string stringStartNameWithStyles = "\"value\":\"Styles:\"";
            const string stringStartName = "\"value\":\"";
            const string stringEndName = "\"";

            var descriptions = new List<string>();

            var startName = substr.IndexOf(stringStartNameWithStyles, StringComparison.Ordinal);
            while (true)
            {
                startName = substr.IndexOf(stringStartName, startName + 1, StringComparison.Ordinal);
                if (startName <= -1) break;
                startName += stringStartName.Length;

                var endName = substr.IndexOf(stringEndName, startName + 1, StringComparison.Ordinal);

                var description = substr.Substring(startName, endName - startName);
                description = description.Trim(' ');
                descriptions.Add(description);
            }
            return descriptions;
        }

        private static List<string> ParseSockets(int length, string substr)
        {
            const string stringEndPossibleSocket = "<\\/span><\\/div><\\/div>";
            const string stringStartSocket = ">";
            const string stringSocket = "Kinetic Gem";
            const string stringEndSocket = "<\\/span><br>";

            var sockets = new List<string>();
            while (true)
            {
                length = substr.LastIndexOf(stringEndPossibleSocket, length, StringComparison.Ordinal);

                if (length == -1) break;

                var startLength = substr.LastIndexOf(stringStartSocket, length, StringComparison.Ordinal);

                if (startLength == -1)
                {
                    break;
                }

                var socketName = substr.Substring(startLength + 1, length - startLength);

                if (socketName == stringSocket)
                {
                    length = substr.LastIndexOf(stringEndSocket, length, StringComparison.Ordinal) - 1;

                    startLength = substr.LastIndexOf(stringStartSocket, length - stringSocket.Length, StringComparison.Ordinal);

                    socketName = substr.Substring(startLength, length - startLength);
                }

                sockets.Add(socketName);
            }

            sockets.Reverse();
            return sockets;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _itemName.ChangeItemNameForSite();

            Items = new List<Item>();

            DescriptionFilter = _rigthSocket is null && _rigthDescription != null
                ? $@"{_rigthDescription.ChangeDescriptionForSite()}+NOT+locked"
                : _rigthSocket != null
                    ? $"{_rigthSocket.ChangeDescriptionForSite()}"
                    : throw new Exception("��������� ������ ��� ��������");
        }
    }
}