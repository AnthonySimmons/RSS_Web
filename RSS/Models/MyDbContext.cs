using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;


namespace RSS.Models
{
    public class ContextModelChangeInitializer : DropCreateDatabaseIfModelChanges<MyDbContext>
    {
    

        protected override void Seed(MyDbContext context)
        {
            base.Seed(context);


            /*context.chatRooms.Add(new ChatRoom()
            {
                name = "General",
                UserCount = 1
                
            });
            
            context.chatRooms.Add(new ChatRoom()
            {
                UserCount = 0,
                name = "General"
            });
            context.currentRoom = context.chatRooms.FirstOrDefault();
            context.SaveChanges();*/
        }
    }


    public class MyDbContext : DbContext
    {
        private void init()
        {
            //calling this will tell EF to automatically delete the 
            //actual DB whenever a change is detected.  Very handy for debugging, but not so
            //much for production.
            Database.SetInitializer<MyDbContext>(new ContextModelChangeInitializer());
        }
        public MyDbContext()
        {
            init();
            
        }

        public MyDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            init();
            
        }
        public DbSet<channel> channels { get; set; }
        public DbSet<RSSUser> users { get; set; }
        public RSSUser currentUser { get; set; }
        public DbSet<Categories> categories { get; set; }
        
    }
}