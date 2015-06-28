using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RSS.Models
{
    public class Categories
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public int userId { get; set; }
        public bool isCurrent { get; set; }
    }
}