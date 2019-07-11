using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    //todo class entity for simple todo app example
    public class Todo
    {
        public string TodoId { get; set; }
        [Required]
        [Display(Name = "Todo Item")]
        public string TodoItem { get; set; }
        [Display(Name = "Is Done?")]
        public bool IsDone { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
