using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CmsShoppingCart.Models.Data;

namespace CmsShoppingCart.Models.ViewModels.Pages
{
    public class SidebarVM
    {
        public int Id { get; set; }
        [Display(Name = "Sidebar")]
        [AllowHtml]
        public string Body { get; set; }

        public SidebarVM()
        {
            
        }

        public SidebarVM(SidebarDto row)
        {
            Id = row.Id;
            Body = row.Body;
        }
    }
}