using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace RSS
{
    public class parser
    {
        public List<channel> channels = new List<channel>();
        
        [Key]
        public int id { get; set; }
        public int userId { get; set; }

        public void parseAllFeeds()
        { 
            foreach(channel ch in channels)
            {
                loadAnyVersion(ch);
            }
        }

        public void parseOneFeed(string title)
        { 
            channel ch = channels.Where(c => c.title == title).FirstOrDefault();
            if(ch != null)
            {
                loadAnyVersion(ch);
            }
        }

        public void setChannelFeed(string title, string feed)
        {
            foreach (channel ch in channels)
            {
                if (ch.title == title)
                {
                    ch.feed = feed;
                }
            }
        }

        public void removeChannel(string name)
        {
            for (int i = 0; i < channels.Count; i++)
            {
                if(name == channels[i].title)
                {
                    channels.RemoveAt(i);
                    break;
                }
            }

        }

        public int getVersion(string rssString)
        {
            int version = 0;

            //string[] strArr = System.Text.RegularExpressions.Regex.Split(rssString, " ");
            for (int i = 0; i < rssString.Length - 7; i++)
            {
                if (rssString.Substring(i, 4) == "<rss")
                {
                    version = 2;
                }
                if (rssString.Substring(i, 4) == "<rdf")
                {
                    version = 1;
                }
            }

            return version;
        }
        private string processDescription(string description, Item it)
        {
            string newDescrip = "";
            List<string> innerTags = new List<string>();
            string cTag = "";
            bool tag = false;
            for (int i = 0; i < description.Length; i++)
            {
                if (description[i] == '<')
                {
                    cTag = "<";
                    tag = true;
                }
                else if (description[i] == '>')
                {
                    cTag += ">";
                    innerTags.Add(cTag);
                    tag = false;
                }
                else if (!tag)
                {
                    newDescrip += description[i];
                }
                else if(tag == true)
                {
                    cTag += description[i].ToString();
                }
            }
            newDescrip = newDescrip.Replace("&nbsp", "");
            newDescrip = newDescrip.Replace(";", "");
            it.descriptionI = newDescrip;
            it.iframe = innerTags.Where(i => i.Contains("iframe")).FirstOrDefault();
            it.img = innerTags.Where(i => i.Contains("img")).FirstOrDefault();
            if (it.iframe != null) { it.iframe = it.iframe.Replace("\\", "");}
            if (it.img != null) { it.img = it.img.Replace("\\", ""); }
            return newDescrip;
        }

        public bool loadAnyVersion(channel ch)
        {
            //loadXMLRSS1_0(rss_sub);
            ch.item = new List<Item>();
            string rss;
            WebClient wc = new WebClient();
            //if (rss_sub.get_name() == "Email")
            //{
            //    wc.Credentials = new NetworkCredential("wsu.shire", "hackathon");
            //}

            try
            {
                Stream st = wc.OpenRead(ch.link);
                
                using (StreamReader sr = new StreamReader(st))
                {
                    rss = sr.ReadToEnd();
                }

                //version=0 -> Atom 1.0
                //version=1 -> RSS 1.0
                //version=2 -> RSS 2.0
                int version = getVersion(rss);
                if (version == 0)
                {
                    loadXMLAtom(ch, rss);
                }
                else if (version == 1)
                {
                    loadXMLRSS1_0(ch, rss);
                }
                else
                {
                    loadXMLRSS2_0(ch, rss);
                }

                return true;
            }
            catch (System.Net.WebException) { return false; }
        }

        public channel findChannelName(string name)
        {
            foreach (channel ch in channels)
            {
                if (name == ch.title)
                {
                    return ch;
                }
            }
            return null;
        }
        public bool loadXMLRSS2_0(channel ch, string rss)
        {
            
            using (DataSet rssData = new DataSet())
            {
                System.IO.StringReader sr = new System.IO.StringReader(rss);
                DataSet ds2 = new DataSet();

                rssData.ReadXmlSchema("RSS-2_0-Schema.xsd");
                //rssData.InferXmlSchema(sr, null);
                rssData.EnforceConstraints = false;
                rssData.ReadXml(sr, XmlReadMode.Auto);
                string str = rssData.GetXmlSchema();
                if (rssData.Tables.Contains("channel"))
                {
                    foreach (DataRow dataRow in rssData.Tables["channel"].Rows)
                    {
                        
                        //ch.title = dataRowContains("title", dataRow, rssData);//Convert.ToString(dataRow["title"]);

                        //rss_sub.set_title(ch.title);
                        ch.description = dataRowContains("description", dataRow, rssData);//Convert.ToString(dataRow["description"]);
                        ch.link = dataRowContains("link", dataRow, rssData);//Convert.ToString(dataRow["link"]);
                        ch.lastBuildDate = dataRowContains("lastBuildDate", dataRow, rssData);//Convert.ToString(dataRow["lastBuildDate"]);
                        ch.pubDate = dataRowContains("pubDate", dataRow, rssData);//Convert.ToString(dataRow["pubDate"]);
                        ch.ttl = dataRowContains("ttl", dataRow, rssData);//Convert.ToString(dataRow["ttl"]);

                        

                        foreach (DataRow im in rssData.Tables["image"].Rows)
                        {
                            ch.imageUrl = im["url"].ToString();
                            ch.imageTitle = im["title"].ToString();
                        }

                        
                        int counter = 0;
                        if (ch.pubDate.Length > 6)
                        {
                            ch.pubDate = ch.pubDate.Substring(0, ch.pubDate.Length - 6);
                        }
                        foreach (DataRow itemRow in rssData.Tables["item"].Rows)
                        {
                            Item inside = new Item();
                            inside.titleI = dataRowContains("title", itemRow, rssData);//Convert.ToString(itemRow["title"]);
                            string desc = dataRowContains("description", itemRow, rssData);// Convert.ToString(itemRow["description"]);
                            processDescription(desc, inside);
                            inside.linkI = dataRowContains("link", itemRow, rssData);//Convert.ToString(itemRow["link"]);
                            inside.guidI = dataRowContains("guid", itemRow, rssData);//Convert.ToString(itemRow["guid"]);
                            //inside.guidI = Convert.ToString(rssData.Tables["guid"].Rows[counter].ItemArray[1]);                        
                            inside.pubDateI = dataRowContains("pubDate", itemRow, rssData);//Convert.ToString(itemRow["pubDate"]);
                            if(inside.pubDateI.Length > 6)
                            {
                                inside.pubDateI = inside.pubDateI.Substring(0, inside.pubDateI.Length - 6);
                            }
                            
                            inside.subscription = ch.title;
                            ch.item.Add(inside);
                            counter++;
                            if (counter > ch.maxItems) { break; }
                        }
                        /*channel rem = channels.Where(c => c.title == name).FirstOrDefault();
                        if (rem != null)
                        {
                            channels.Remove(rem);
                        }
                        if (findChannelName(ch.title) == null)
                        {
                            
                            channels.Add(ch);
                        }
                        */
                    }
                }
                return true;
            }
        }


        public bool loadXMLRSS1_0(channel ch, string rss)
        {
            using (DataSet rssData = new DataSet())
            {
                System.IO.StringReader sr = new System.IO.StringReader(rss);
                DataSet ds2 = new DataSet();

                //rssData.ReadXmlSchema("../../RSS-1_0-Schema.xsd");
                rssData.ReadXml(sr, XmlReadMode.InferSchema);
                if (rssData.Tables.Contains("channel"))
                {
                    foreach (DataRow dataRow in rssData.Tables["channel"].Rows)
                    {
                        ch.title = dataRowContains("title", dataRow, rssData);//Convert.ToString(dataRow["title"]);
                        //rss_sub.set_title(ch.title);
                        ch.description = dataRowContains("description", dataRow, rssData);//Convert.ToString(dataRow["description"]);
                        ch.link = dataRowContains("link", dataRow, rssData);//Convert.ToString(dataRow["link"]);
                        int counter = 0;
                        if (rssData.Tables.Contains("item"))
                        {
                            foreach (DataRow itemRow in rssData.Tables["item"].Rows)
                            {
                                Item inside = new Item();
                                inside.titleI = dataRowContains("title", itemRow, rssData);//Convert.ToString(itemRow["title"]);
                                inside.descriptionI = dataRowContains("description", itemRow, rssData);//Convert.ToString(itemRow["description"]);
                                inside.linkI = dataRowContains("link", itemRow, rssData);//Convert.ToString(itemRow["link"]);
                                inside.guidI = dataRowContains("guid", itemRow, rssData);//Convert.ToString(itemRow["guid"]);
                                //inside.guidI = Convert.ToString(rssData.Tables["guid"].Rows[counter].ItemArray[1]);                        
                                inside.pubDateI = dataRowContains("pubDate", itemRow, rssData);//Convert.ToString(itemRow["pubDate"]);
                                inside.subscription = ch.title;
                                ch.item.Add(inside);
                                counter++;
                                if (counter > ch.maxItems) { break; }
                            }
                        }
                        /*if (findChannelName(ch.title) == null)
                        {
                            channels.Add(ch);
                        }*/
                    }
                }
                return true;
            }
        }
        public string dataRowContains(string name, DataRow dataRow, DataSet ds)
        {
            if (dataRow.Table.Columns.Contains(name))
            {
                return dataRow[name].ToString();
            }
            else if (ds.Tables.Contains(name))
            {
                string str = ds.Tables[name].ToString();
                string s = "";
                return s;
            }
            else
            {
                return "";
            }
        }
        public bool loadXMLAtom(channel ch, string rss)
        {
            using (DataSet rssData = new DataSet())
            {
                System.IO.StringReader sr = new System.IO.StringReader(rss);
                DataSet ds2 = new DataSet();
                rssData.ReadXml(sr, XmlReadMode.Auto);
                string str = rssData.GetXmlSchema();

                if (rssData.Tables.Contains("feed"))
                {
                    foreach (DataRow dataRow in rssData.Tables["feed"].Rows)
                    {
                        int c = rssData.Tables.Count;
                        
                        if (rssData.Tables.Contains("link"))
                        {
                            foreach (DataRow dr in rssData.Tables["link"].Rows)
                            {
                                ch.link = dataRowContains("href", dr, rssData);
                                break;
                            }
                        }
                        //dataRowContains("title", dataRow, rssData);
                        //rss_sub.set_title(ch.title);
                        ch.description = dataRowContains("subtitle", dataRow, rssData);//Convert.ToString(dataRow["subtitle"]);
                        ch.pubDate = dataRowContains("updated", dataRow, rssData);//Convert.ToString(dataRow["updated"]);
                        int counter = 0;
                        if (rssData.Tables.Contains("entry"))
                        {
                            foreach (DataRow itemRow in rssData.Tables["entry"].Rows)
                            {
                                Item inside = new Item();
                                inside.titleI = dataRowContains("title", itemRow, rssData);//Convert.ToString(itemRow["title"]);
                                inside.descriptionI = dataRowContains("summary", itemRow, rssData);//Convert.ToString(itemRow["summary"]);
                                inside.linkI = dataRowContains("id", itemRow, rssData);//Convert.ToString(rssData.Tables["id"]);
                                inside.pubDateI = dataRowContains("updated", itemRow, rssData);//Convert.ToString(itemRow["updated"]);
                                if (inside.pubDateI == "")
                                {
                                    inside.pubDateI = dataRowContains("issued", itemRow, rssData);
                                }
                                inside.subscription = ch.title;
                                if (rssData.Tables.Contains("link"))
                                {
                                    foreach (DataRow dr in rssData.Tables["link"].Rows)
                                    {
                                        if (dr["href"].ToString().Contains(".html"))
                                        {
                                            inside.linkI = dr["href"].ToString();
                                        }
                                    }
                                }
                                if (rssData.Tables.Contains("author"))
                                {
                                    //foreach (DataRow authorRow in rssData.Tables["author"].Rows)
                                    DataRow authorRow = rssData.Tables["author"].Rows[0];
                                    //foreach (DataRow authorRow in itemRow["author"])
                                    {
                                        author auth = new author();
                                        auth.name = dataRowContains("name", authorRow, rssData);
                                        auth.email = dataRowContains("email", authorRow, rssData);
                                        inside.authors.Add(auth);
                                    }
                                }
                                ch.item.Add(inside);
                                
                                counter++;
                                if (counter > ch.maxItems) { break; }
                            }
                        }
                        /*if (findChannelName(ch.title) == null)
                        {
                            channels.Add(ch);
                        }*/
                    }

                }
                return true;
            }
        }
    }
}
