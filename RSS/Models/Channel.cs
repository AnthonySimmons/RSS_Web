using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace RSS
{
        public class channel
        {
            [XmlAttribute("Feed")]
            public string feed {get; set;}
            [XmlAttribute("version")]
            public string version { get; set; }
            [XmlElement("title")]
            public string title { get; set; }
            [XmlElement("description")]
            public string description { get; set; }
            [XmlElement("link")]
            public string link { get; set; }
            [XmlElement("lastBuildDate")]
            public string lastBuildDate { get; set; }
            [XmlElement("pubDate")]
            public string pubDate { get; set; }
            [XmlElement("ttl")]
            public string ttl { get; set; }
            [XmlElement("update")]
            public int update { get; set; }
            [XmlAttribute("item")]
            public List<Item> item = new List<Item>();
            [XmlAttribute("imageUrl")]
            public string imageUrl { get; set; }
            [XmlAttribute("imageTitle")]
            public string imageTitle { get; set; }
            
            public int maxItems { get; set; }

            
            public int categoryId { get; set; }

            public bool isCurrent { get; set; }
            [Key]
            public int id { get; set; }

            
            public int userId { get; set; }

            public string readString { get; set; }


            public channel()
            {
                maxItems = 4;
            }
            public void buildReadString()
            {
                int count = 0;
                if(item != null)
                {
                    this.readString += this.title + " " + this.description + ". ";
                    foreach (var it in item)
                    {
                        this.readString += "Item " + count.ToString() + ". " + it.titleI + " " + it.pubDateI + " " + it.descriptionI + " ";
                        count++;
                    }
                }
            }

            public bool isMatch(channel source)
            { 
                return (this.link == source.link && this.title == source.title && this.description == source.description);
            }

            public int findIndex(Item it)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    if (it.isMatch(item[i]))
                    {
                        return i;
                    }
                }
                return -1;
            }


            public void removeItem(Item it)
            {
                int index = findIndex(it);
                item.RemoveAt(index);
            }
    }
}
