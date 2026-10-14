using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB_453505_Shirko.Domain.Entities
{
    internal class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public List<Bike> Bikes { get; set; }
    }
}
