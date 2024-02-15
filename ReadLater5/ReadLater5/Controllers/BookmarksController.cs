using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Entity;
using Microsoft.AspNetCore.Identity;

namespace ReadLater5.Controllers
{
    public class BookmarksController : Controller
    {
        private readonly ReadLaterDataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookmarksController(ReadLaterDataContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bookmarks
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Redirect("Identity/Account/Login");

            var bookmarks = await _context.Bookmark.Include(b => b.Category).Where(b => b.UserId == user.Id).ToListAsync();

            return View(bookmarks);
        }

        // GET: Bookmarks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var bookmark = await _context.Bookmark
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (bookmark == null)
                return NotFound();

            return View(bookmark);
        }

        // GET: Bookmarks/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "ID", "Name");
            return View();
        }

        // POST: Bookmarks/Create
        [HttpPost]
        public async Task<IActionResult> CreateBookmarkWithCategory(Bookmark bookmark, string newCategory)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Redirect("Identity/Account/Login");


            if (!string.IsNullOrEmpty(newCategory))
            {
                // Check if the category already exists for the current user
                var existingCategory = _context.Categories
                    .FirstOrDefault(c => c.Name == newCategory && c.UserId == currentUser.Id);

                if (existingCategory == null)
                {
                    // Create a new category for the current user
                    var category = new Category { Name = newCategory, UserId = currentUser.Id };
                    _context.Categories.Add(category);
                    _context.SaveChanges();

                    bookmark.CategoryId = category.ID;
                }
                else
                {
                    bookmark.CategoryId = existingCategory.ID;
                }
            }

            bookmark.UserId = currentUser.Id;
            bookmark.CreateDate = DateTime.Now;

            _context.Bookmark.Add(bookmark);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: Bookmarks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var bookmark = await _context.Bookmark.FindAsync(id);
            if (bookmark == null)
                return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "ID", "Name");
            return View(bookmark);
        }

        // POST: Bookmarks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,URL,ShortDescription,CategoryId,CreateDate,UserId")] Bookmark bookmark)
        {
            if (id != bookmark.ID)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Redirect("Identity/Account/Login");

            if (ModelState.IsValid)
            {
                try
                {
                    bookmark.UserId = currentUser.Id;

                    _context.Update(bookmark);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookmarkExists(bookmark.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "ID", "ID", bookmark.CategoryId);
            return View(bookmark);
        }

        // GET: Bookmarks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var bookmark = await _context.Bookmark
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (bookmark == null)
                return NotFound();

            return View(bookmark);
        }

        // POST: Bookmarks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookmark = await _context.Bookmark.FindAsync(id);
            _context.Bookmark.Remove(bookmark);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool BookmarkExists(int id)
        {
            return _context.Bookmark.Any(e => e.ID == id);
        }
    }
}
