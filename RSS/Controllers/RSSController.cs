using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSS.Models;

namespace RSS.Controllers
{
    public class RSSController : HomeController
    {
        //
        // GET: /RSS/
        public ActionResult RSSIndex()
        {
            int userid = (int)Session["userid"];
            RSSUser user = Db.users.Where(c => c.Id == userid).FirstOrDefault();
            parser Pars = new parser();

            if (user != null)
            {
                user.subscriptions = Db.channels.Where(c => c.userId == user.Id).ToList();
                user.currentChannel = Db.channels.Where(c => c.isCurrent == true).FirstOrDefault();
                user.categories = Db.categories.Where(c => c.userId == user.Id).ToList();

                user.currentCategory = Db.categories.Where(c => c.isCurrent == true).FirstOrDefault();
            }

            Pars.channels = user.subscriptions;
            if (user.currentChannel != null)
            {
                //user.currentChannel.item = Db.articles.Where(a => a.channelId == user.currentChannel.id).ToList();
                if (user.currentChannel.item.Count == 0)
                {
                    Pars.parseOneFeed(user.currentChannel.title);
                    channel ch = Pars.channels.Where(c => c.title == user.currentChannel.title).FirstOrDefault();
                    if (ch != null)
                    {
                        user.currentChannel.item = ch.item;
                    }
                    
                    user.currentArticle = (string)Session["currentarticle"];
                    
                }
            
            }
            if (user.currentChannel == null) { user.currentChannel = new channel(); }


            return View("RSSIndex", user);
        }



        public ActionResult SetCurrentArticle(string itemLink)
        {
            Session["currentarticle"] = itemLink;
            return RedirectToAction("RSSIndex");
        }


        public ActionResult SetCurrent(string nm)
        {
            int userid = (int)Session["userid"];
            if (Db.currentUser == null)
            {
                Db.currentUser = Db.users.Where(c => c.Id == userid).FirstOrDefault();
                Db.currentUser.subscriptions = Db.channels.Where(c => c.userId == Db.currentUser.Id).ToList();
            }
            foreach (var c in Db.channels)
            {
                c.isCurrent = false;
            }
            Session["currentarticle"] = "";
            Db.currentUser.currentArticle = "";
            Db.currentUser.currentChannel = Db.currentUser.subscriptions.Where(n => n.title == nm).FirstOrDefault();
            Db.currentUser.currentChannel.isCurrent = true;
            Db.Entry(Db.currentUser).State = System.Data.Entity.EntityState.Modified;
            Db.currentUser.categories = Db.categories.Where(c => c.userId == userid).ToList();
            Db.SaveChanges();

            return RedirectToAction("RSSIndex");
        }

        [HttpGet]
        public ActionResult GetFeedList()
        {
            int userid = (int)Session["userid"];
            RSSUser user = Db.users.Where(c => c.Id == userid).FirstOrDefault();
         
            if (user != null)
            {
                user.subscriptions = Db.channels.Where(c => c.userId == user.Id).ToList();
                user.currentChannel = Db.channels.Where(c => c.isCurrent == true).FirstOrDefault();
                user.categories = Db.categories.Where(c => c.userId == user.Id).ToList();

                user.currentCategory = Db.categories.Where(c => c.isCurrent == true).FirstOrDefault();
            }
            return PartialView("FeedList", user);
        }


        public ActionResult AddFeed(string url, string name, string cat)
        {
            int userid = (int)Session["userid"];
            if (Db.currentUser == null)
            {
                Db.currentUser = Db.users.Where(c => c.Id == userid).FirstOrDefault();
            }
            Categories category = Db.categories.Where(c => c.name == cat).FirstOrDefault();
            if (category == null)
            {
                category = new Categories();
                category.name = cat;
                category.userId = userid;
                Db.categories.Add(category);
                Db.SaveChanges();
            }
            
            channel newCh = new channel();
            newCh.categoryId = category.id;
            newCh.title = name;
            newCh.link = url;
            newCh.userId = userid;
            Db.channels.Add(newCh);
            //Db.Entry(newCh).State = System.Data.Entity.EntityState.Modified;
            Db.SaveChanges();

            return RedirectToAction("RSSIndex");
        }


        public ActionResult UpdateFeeds()
        {
            
            channel ch = Db.channels.Where(c => c.isCurrent == true).FirstOrDefault();

            if (ch != null)
            {
                parser p = new parser();
                p.parseAllFeeds();
                //channel chan = Pars.channels.Where(c => c.title == user.currentChannel.title).FirstOrDefault();
                if (ch != null)
                {
                  //  user.currentChannel.item = ch.item;
                }
            }

            return RedirectToAction("RSSIndex");
        }

        public ActionResult LoadMore(int cid)
        {
            channel ch = Db.channels.Where(c => c.id == cid).FirstOrDefault();
            if (ch != null)
            {
                ch.maxItems += 5;
                Db.Entry(ch).State = System.Data.Entity.EntityState.Modified;
                Db.SaveChanges();
            }

            //parser mp = new parser();
            //mp.parseOneFeed(ch.title);
            return RedirectToAction("RSSIndex");
        }

        public ActionResult ExpandCategory(int cid)
        {
            int userid = (int)Session["userid"];
            Categories cat = Db.categories.Where(c => c.id == cid).FirstOrDefault();
            Db.currentUser = Db.users.Where(u => u.Id == userid).FirstOrDefault();
            if (Db.currentUser != null)
            {
                Db.currentUser.categories = Db.categories.Where(c => c.userId == userid).ToList();
                Db.currentUser.currentCategory = Db.categories.Where(c => c.isCurrent == true).FirstOrDefault();
                Db.currentUser.subscriptions = Db.channels.Where(ch => ch.userId == userid).ToList();
            }

            if (cat.isCurrent == true)
            {
                cat.isCurrent = false;
            }
            else
            {
                foreach (var c in Db.categories)
                {
                    c.isCurrent = false;
                }
                cat.isCurrent = true;
            }
            Db.Entry(cat).State = System.Data.Entity.EntityState.Modified;
            Db.SaveChanges();
            //return RedirectToAction("RSSIndex");
            return PartialView("FeedList", Db.currentUser);
        }
        public ActionResult EditChannel(int channelId, string nName)
        {
            channel ch = Db.channels.Where(c => c.id == channelId).FirstOrDefault();

            if (ch != null)
            {
                ch.title = nName;
                Db.Entry(ch).State = System.Data.Entity.EntityState.Modified;
                Db.SaveChanges();
            }

            return RedirectToAction("RSSIndex");
        }

        public ActionResult RemoveChannel(int channelId)
        {
            //int uid = (int)Session["userid"];
            
            channel ch = Db.channels.Where(c => c.id == channelId).FirstOrDefault();
            
            //RSSUser user = Db.users.Where(u => u.Id == uid).FirstOrDefault();
            if (ch != null)
            {
                Categories cat = Db.categories.Where(c => ch.categoryId == c.id).FirstOrDefault();

                /*if (cat.isCurrent == true)
                {
                    cat.isCurrent = false;
                    ch.isCurrent = false;
                    Db.Entry(cat).State = System.Data.Entity.EntityState.Modified;
                    Db.SaveChanges();
                    return RedirectToAction("RSSIndex");
                }*/

                if (ch.isCurrent == true)
                {
                    ch.isCurrent = false;
                    Db.Entry(ch).State = System.Data.Entity.EntityState.Modified;
                    Db.SaveChanges();
                    return RedirectToAction("RSSIndex");
                }

                //ch.isCurrent = false;

                //ch.categoryId = 0;
                ch.userId = 0;
                ch.categoryId = 0;
                Db.Entry(ch).State = System.Data.Entity.EntityState.Modified;
                
                if (cat != null)
                {
                    if (Db.channels.Where(c => c.categoryId == cat.id).ToList().Count == 1)
                    {                     
                        cat.isCurrent = false;
                        Db.Entry(cat).State = System.Data.Entity.EntityState.Deleted;       
                    }
                }
                Db.SaveChanges();
            }
            return RedirectToAction("RSSIndex");
        }
	}
}