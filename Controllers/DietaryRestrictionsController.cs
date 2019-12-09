using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampOrno.Data;
using CampOrno.Models;
using Microsoft.AspNetCore.Authorization;

namespace CampOrno.Controllers
{
    [Authorize]
    public class DietaryRestrictionsController : Controller
    {
        private readonly CampOrnoContext _context;

        public DietaryRestrictionsController(CampOrnoContext context)
        {
            _context = context;
        }

        // GET: DietaryRestrictions
        public async Task<IActionResult> Index()
        {
            var dietaryRestrictions = from c in _context.DietaryRestrictions
                            .Include(a => a.CamperDiets).ThenInclude(cc => cc.Camper)
                            select c;
            return View(await dietaryRestrictions.ToListAsync());
        }

        // GET: DietaryRestrictions/Details/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dietaryRestriction = await _context.DietaryRestrictions
                .FirstOrDefaultAsync(m => m.ID == id);
            if (dietaryRestriction == null)
            {
                return NotFound();
            }

            return View(dietaryRestriction);
        }

        // GET: DietaryRestrictions/Create
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: DietaryRestrictions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("ID,Name")] DietaryRestriction dietaryRestriction)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(dietaryRestriction);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(dietaryRestriction);
        }

        // GET: DietaryRestrictions/Edit/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dietaryRestriction = await _context.DietaryRestrictions.FindAsync(id);
            if (dietaryRestriction == null)
            {
                return NotFound();
            }
            return View(dietaryRestriction);
        }

        // POST: DietaryRestrictions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var dietaryRestrictionToUpdate = await _context.DietaryRestrictions.FindAsync(id);
            if (dietaryRestrictionToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<DietaryRestriction>(dietaryRestrictionToUpdate, "",
                p => p.Name))
            {
                try
                {
                    _context.Update(dietaryRestrictionToUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DietaryRestrictionExists(dietaryRestrictionToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(dietaryRestrictionToUpdate);
        }

        // GET: DietaryRestrictions/Delete/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dietaryRestriction = await _context.DietaryRestrictions
                .FirstOrDefaultAsync(m => m.ID == id);
            if (dietaryRestriction == null)
            {
                return NotFound();
            }

            return View(dietaryRestriction);
        }

        // POST: DietaryRestrictions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dietaryRestriction = await _context.DietaryRestrictions.FindAsync(id);
            try
            {
                _context.DietaryRestrictions.Remove(dietaryRestriction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.InnerException.Message.Contains("FK_CamperDiets_DietaryRestrictions_DietaryRestrictionID"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, you cannot delete a dietary restriction that any campers have noted.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(dietaryRestriction);
        }

        private bool DietaryRestrictionExists(int id)
        {
            return _context.DietaryRestrictions.Any(e => e.ID == id);
        }
    }
}
