using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Entity;

namespace ReadLater5.Controllers
{
    public class BookmarksController : Controller
    {
        private readonly ReadLaterDataContext _context;

        public BookmarksController(ReadLaterDataContext context)
        {
            _context = context;
        }

        // GET: Bookmarks
        public async Task<IActionResult> Index()
        {
            var readLaterDataContext = _context.Bookmark.Include(b => b.Category);

            return View(await readLaterDataContext.ToListAsync());
        }

        // GET: Bookmarks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookmark = await _context.Bookmark
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bookmark == null)
            {
                return NotFound();
            }

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
        public IActionResult CreateBookmarkWithCategory(Bookmark bookmark, string newCategory)
        {
            // Check if the category already exists
            var existingCategory = _context.Categories.FirstOrDefault(c => c.Name == newCategory);

            if (existingCategory == null)
            {
                // Create a new category
                var category = new Category { Name = newCategory };
                _context.Categories.Add(category);
                _context.SaveChanges();

                bookmark.CategoryId = category.ID;
            }
            else
            {
                bookmark.CategoryId = existingCategory.ID;
            }

            bookmark.CreateDate = DateTime.Now;
            _context.Bookmark.Add(bookmark);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Bookmarks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookmark = await _context.Bookmark.FindAsync(id);
            if (bookmark == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.Categories, "ID", "Name");
            return View(bookmark);
        }

        // POST: Bookmarks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,URL,ShortDescription,CategoryId,CreateDate")] Bookmark bookmark)
        {
            if (id != bookmark.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
            {
                return NotFound();
            }

            var bookmark = await _context.Bookmark
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bookmark == null)
            {
                return NotFound();
            }

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
