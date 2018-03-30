using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //Declare a list of models
            List<CategoryVM> categoryVMList;

            using (Db db=new Db())
            {
                //Init the list
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x))
                    .ToList();
            }

            //Return the view with the list
            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Declare id
            string id;

            using (Db db=new Db())
            {
                //Check that the category name is unique
                if (db.Categories.Any(x => x.Name == catName))
                {
                    return "titletaken";
                }

                //Init DTO
                CategoryDto dto=new CategoryDto();

                //Add to DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //Save DTO
                db.Categories.Add(dto);
                db.SaveChanges();

                //Get the id
                id = dto.Id.ToString();
            }

            //Return id
            return id;
        }

        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Set initial count
                int count = 1;

                //Declare CategoryDto
                CategoryDto dto;

                //Set sorting for each page
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Get the page
                CategoryDto dto = db.Categories.Find(id);

                //Remove the page
                db.Categories.Remove(dto);

                //Save
                db.SaveChanges();
            }

            //Redirect
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db=new Db())
            {
                //Check category name is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                //Get the DTO
                CategoryDto dto = db.Categories.Find(id);

                //Edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //Save
                db.SaveChanges();
            }

            //Return
            return "ok";
        }

        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Init the model
            ProductVM model=new ProductVM();

            //Add select list of categories to the model
            using (Db db=new Db())
            {
                model.Categories=new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            //Return the view with model
            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                using (Db db=new Db())
                {
                    //We must populate list of categories before return view with model
                    model.Categories=new SelectList(db.Categories.ToList(),"Id", "Name");
                    return View(model);
                }
            }

            //Make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    //We must populate list of categories before return view with model
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Product name is taken!");
                    return View(model);
                }
            }

            //Declare product id
            int id;

            //Init and save ProductDto
            using (Db db=new Db())
            {
                ProductDto product=new ProductDto();
                product.Name = model.Name;
                //Check for and set slug if need be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    product.Slug = model.Name.Replace(" ", "-").ToLower();
                }
                else
                {
                    product.Slug = model.Slug.Replace(" ", "-").ToLower();
                }
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDto catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                model.CategoryName = catDto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //Get inserted id
                id = product.Id;
            }

            //Set TemData message
            TempData["SM"] = "You've added a product!";

            #region Upload Image

            //Create the neccessary directories
            var originalDirectory = new DirectoryInfo(String.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString()+"\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString()+"\\Galery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString()+ "\\Galery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Check if file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                //Get file extension
                string ext = file.ContentType.ToLower();

                //Verify file extension
                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" && ext != "image/gif" &&
                    ext != "image/x-png" && ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                            //We must populate list of categories before return view with model
                            model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                            ModelState.AddModelError("", "The image was not uploaded - wrong image extension!");
                            return View(model);
                    }
                }

                //Init image name
                string imageName = file.FileName;

                //Save image name to DTO
                using (Db db=new Db())
                {
                    ProductDto dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //Set original and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //Save original
                file.SaveAs(path);

                //Create and save thumb
                WebImage img=new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            //Redirect
            return RedirectToAction("AddProduct");
        }
    }
}