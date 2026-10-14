using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB_453505_Shirko.Domain.Entities
{
    public class Bike
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public float Price { get; set; }
        public string? Image {  get; set; }
        public string? MimeType { get; set; }


    }
}
