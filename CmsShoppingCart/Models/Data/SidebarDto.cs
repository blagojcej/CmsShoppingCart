using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CmsShoppingCart.Models.Data
{
    [Table("tblSidebar")]
    public class SidebarDto
    {
        [Key]
        public int Id { get; set; }
        public string Body { get; set; }
    }
}