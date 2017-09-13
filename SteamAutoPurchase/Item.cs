using System.Collections.Generic;

namespace TestBed
{
    public class Item
    {
        public int Index { get; set; }

        public string Id { get; set; }

        public Price Price { get; set; }

        public List<string> Sockets { get; set; }
        public List<string> Descriptions { get; set; }

        public Item()
        {
            Sockets = new List<string>();
            Descriptions = new List<string>();
        }
    }
}