using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RSS
{
    [Serializable()]
    public class Item
    {
        [Key]
        public int id { get; set; }
        public int channelId { get; set; }
        [XmlElement("title")]
        public string titleI { get; set; }
        [XmlElement("description")]
        public string descriptionI { get; set; }
        [XmlElement("link")]
        public string linkI { get; set; }
        [XmlElement("guid")]
        public string guidI { get; set; }
        [XmlElement("pubDate")]
        public string pubDateI { get; set; }
        public List<author> authors { get; set; }
        public bool read = false;
        public string subscription { get; set; }

        public string img { get; set; }
        public string iframe { get; set; }

        public bool isMatch(Item source)
        {
            return (this.titleI == source.titleI && this.linkI == source.linkI && this.descriptionI == source.descriptionI);
        }
    }

    [Serializable()]
    public class author
    {
        [XmlElement("Name")]
        public string name;
        [XmlElement("email")]
        public string email;
    }
}
