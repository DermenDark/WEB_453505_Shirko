using Microsoft.AspNetCore.Mvc.Rendering;

namespace LRs.Models
{
    public class IndexViewModel
    {
        public SelectList Items { get; set; }
        public int SelectedId { get; set; }
    }
}