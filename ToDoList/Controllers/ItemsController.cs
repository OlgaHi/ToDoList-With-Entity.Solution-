using Microsoft.AspNetCore.Mvc;
using ToDoList.Models;
using System.Collections.Generic; // to be able to use lists, dictionary
using System.Linq;// ORM, to be able query objects
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; //html helpers
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; //controller can interact with users from the database
using System.Threading.Tasks; //to call async methods
using System.Security.Claims;

namespace ToDoList.Controllers
{
  [Authorize]
  public class ItemsController : Controller
  {
    private readonly ToDoListContext _db;

    private readonly UserManager<ApplicationUser> _userManager;

    public ItemsController(UserManager<ApplicationUser> userManager, ToDoListContext db)
    {
      _userManager = userManager;
      _db = db;
    }

    public async Task<ActionResult> Index()
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      var userItems = _db.Items.Where(entry => entry.User.Id == currentUser.Id).ToList();
      return View(userItems);
    }

    public ActionResult Details(int id)
    {
      var thisItem = _db.Items //list of items
        .Include(item => item.Categories) //collection of join entities, reference to a relationship
        .ThenInclude(join => join.Category) // category of each category item
        .FirstOrDefault(item => item.ItemId == id); // which item
      return View(thisItem);
    }
    
    public ActionResult Create()
    {
    ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
    return View();
    }

    [HttpPost]
    public async Task<ActionResult> Create(Item item, int CategoryId)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      item.User = currentUser;
      _db.Items.Add(item);
      if (CategoryId != 0)
      {
        _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
    _db.SaveChanges();
    return RedirectToAction("Index");
}
    public ActionResult Edit(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(item => item.ItemId == id);
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name") ;
      return View(thisItem);
    }

    [HttpPost]
    public ActionResult Edit(Item item, int CategoryId)
    {
      if (CategoryId != 0)
      {
        _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
      _db.Entry(item).State = EntityState.Modified;
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult AddCategory(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(item => item.ItemId == id);
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View(thisItem);
    }

    [HttpPost]
    public ActionResult AddCategory(Item item, int CategoryId)
    {
      if (CategoryId != 0)
      {
        _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult Delete(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(item => item.ItemId == id);
      return View(thisItem);
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(item => item.ItemId == id);
      _db.Items.Remove(thisItem);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult DeleteCategory(int joinId)
    {
      var joinEntry = _db.CategoryItem.FirstOrDefault(entry => entry.CategoryItemId == joinId);
      _db.CategoryItem.Remove(joinEntry);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }
  }
}
    
