using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SteamAutoPurchase
{
    [CollectionDataContract(Name = "logins", ItemName = "login", Namespace = "")]
    public class Logins : List<ItemPage>
    { }

    [DataContract(Name = "Steam1", Namespace = "")]
    public class SteamMarketplace
    {
        [DataMember(EmitDefaultValue = false)]
        public Logins Itempages { get; private set; }


        public SteamMarketplace(SteamAuth.UserLogin login)
        {
            Itempages = Deserialized();

            Itempages.ForEach(p => p.Login = login);
        }

        private static Logins Deserialized()
        {
            var ser = new DataContractSerializer(typeof(SteamMarketplace), new[] { typeof(Logins), typeof(ItemPage) });

            using (var stream = new System.IO.FileStream("items.xml", System.IO.FileMode.Open))
            {
                return (Logins)ser.ReadObject(stream);
            }
        }

        public async System.Threading.Tasks.Task LoadMarkplaceItemsAsync(System.DateTime startTime)
        {
            if (Itempages.Count == 0) throw new System.Exception("добавьте вещи для покупки");

            while (startTime.AddDays(1) > System.DateTime.Now)
            {
                foreach (var itempage in Itempages)
                {
                    await itempage.LoadMarketplacePageAsync();
                }
            }
        }
    }
}