using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrganicOption.Models.Blogs
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string ShortDescription { get; set; }

        [Required]
        public string FullDescription { get; set; }
        public string Image { get; set; }    
     
        public DateTime PostedDate { get; set; }
    }
}
