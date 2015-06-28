using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSS.Models;
using System.Speech;
using System.Speech.Synthesis;
using System.Speech.Recognition;

namespace RSS.Controllers
{
    public class HomeController : Controller
    {
        public RSS.Models.MyDbContext Db;
        

        public HomeController()
            : this(null)  //note that calling this(null) will also call 
        //code inside HomeController(MyDbContext)
        {

        }

        public HomeController(MyDbContext someDb = null)
        {
            //If we were given a null context, just use the default context
            if (someDb == null)
            {
                Db = new MyDbContext("MyDbContext");
            }
            else
            {
                //otherwise, use the supplied context
                Db = someDb;
            }
        
        }


        public ActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        public ActionResult Login(RSSUser model)
        {
            int uId = 0;
            bool found = false;
            Session["userid"] = null;
            Session["canwrite"] = false;
            Session["isadmin"] = false;
            Session["currentarticle"] = "";
            

            foreach (RSSUser p in Db.users)
            {
                if (model.Email == p.Email && model.Password == p.Password)
                {
                    uId = p.Id;
                    
                    
                    Session["userid"] = p.Id;
                    Session["name"] = p.FirstName + " " + p.LastName;
                    Session["login"] = true;
                    Session["ScrollHeight"] = 0;
                    if (p.isAdmin == true)
                    {
                        Session["isadmin"] = true;
                    }
                    Db.currentUser = p;
                    found = true;


                    if (Db.channels != null)
                    {
                        foreach (channel ch in Db.channels)
                        {
                            ch.isCurrent = false;
                        }
                    }
                    if (Db.users != null)
                    {
                        foreach (RSSUser us in Db.users)
                        {
                            us.currentCategory = null;
                            us.currentChannel = null;
                            us.currentArticle = null;
                        }
                    }
                    if (Db.categories != null)
                    {
                        foreach (Categories cat in Db.categories)
                        {
                            cat.isCurrent = false;
                        }
                    }

                    Db.SaveChanges();
                }
            }
            if (!found)
            {
                foreach (string key in ModelState.Keys)
                {
                    ModelState[key].Errors.Clear();
                }

                ModelState.AddModelError("", "Invalid User Name / Password");
                return View("Login");
            }
            Db.SaveChanges();
            //return RedirectToAction("Login");
            //return View("MyBlogs", new { model = db.CurrentBlogger });
            return RedirectToAction("RSSIndex", "RSS");

        }

        public ActionResult DeleteAccount(int id)
        {
            RSSUser p = Db.users.Where(c => c.Id == id).FirstOrDefault();
            if (p != null)
            {
                Db.Entry(p).State = System.Data.Entity.EntityState.Deleted;
                Db.SaveChanges();
            }
            return RedirectToAction("Login");
        }


        /// <summary>
        /// View method to allow the creation of a new Person in the DB
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            Session["isadmin"] = false;
            Session["login"] = false;
            //return the view with a newly created (and empty) Person object.
            return View("Create", new RSSUser());
        }


        /// <summary>
        /// Postback version of the Create action.  Note that this method will only be
        /// called on HTTP POST messages (as indicated by the [HttpPost] attribute.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(RSSUser model)
        {
            
            if (ModelState.IsValid)
            {
            
                Db.users.Add(model);
                Db.SaveChanges();

                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("", "One or more issues were found with your submission. Please try again.");
            }
            //If we got here, it means that the model's state is invalid.  Simply return
            //to the create page and display any errors.
            return View("Login", model);
        }

        public ActionResult Logout()
        {
            Session["isadmin"] = false;
            Session["login"] = false;
            int uid = (int)Session["userid"];
            RSSUser user = Db.users.Where(c => c.Id == uid).FirstOrDefault();
            
            if (user != null)
            {

            }


            return RedirectToAction("Login");
        }

        public ActionResult Edit(int id = -1)
        {
            if (id < 0)
            {
                id = (int)Session["userid"];
            }
            //Find the person in the DB.  Use the supplied "id" integer as a reference.
            RSSUser somePerson = Db.users
                .Where(p => p.Id == id)     //this line says to find the person whose ID matches our parameter
                .FirstOrDefault();          //FirstOrDefault() returns either a singluar Person object or NULL

            //If we got NULL, it must mean that we were supplied an incorrect ID.  
            //In this case, redirect to HomeController's Index action.
            if (somePerson == null)
            {
                return RedirectToAction("Index");
            }

            //If we're here, then we must have a valid person.  Send to the "Create" view because
            //create and edit are kind of the same thing.  The 2nd parameter is the model that
            //we will be sending to the Create view.
            return View("Create", somePerson);
        }

        /// <summary>
        /// Postback version of the Edit action.  Will be called when the browser sends us information
        /// back to the server.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(RSSUser model)
        {
            //again, check modelstate to make sure everything went okay
            if (ModelState.IsValid)
            {
                //Because the item already exists in the DB, we want to tell EF that
                //one of its models has been changed.  We use this somewhat strange syntax to
                //accomplish this task.
                Db.Entry(model).State = System.Data.Entity.EntityState.Modified;

                //Again, the above command adds the request to a queue.  To execute the queue,
                //we need to call SaveChanges()
                Db.SaveChanges();

                //when complete, redirect to Index
                return RedirectToAction("Index");
            }

            //Things must've went bad, so send back to the Create view.
            return View("Create", model);
        }


        public ActionResult Index()
        {
            return View(Db.users.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }



    }
}