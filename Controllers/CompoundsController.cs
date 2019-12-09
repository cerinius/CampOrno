using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampOrno.Data;
using CampOrno.Models;
using MedicalOffice.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace CampOrno.Controllers
{
    [Authorize]
    public class CompoundsController : Controller
    {
        private readonly CampOrnoContext _context;

        public CompoundsController(CampOrnoContext context)
        {
            _context = context;
        }

        // GET: Compounds
        public async Task<IActionResult> Index()
        {
            var compounds = from c in _context.Compounds
                            .Include(a=>a.Campers)
                            .Include(a => a.CounselorCompounds).ThenInclude(cc => cc.Counselor)
                            select c;
            return View(await compounds.ToListAsync());
        }

        // GET: Compounds/Details/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compound = await _context.Compounds
                .FirstOrDefaultAsync(m => m.ID == id);
            if (compound == null)
            {
                return NotFound();
            }

            return View(compound);
        }

        // GET: Compounds/Create
        [Authorize(Roles =" Supervisor, Admin")]
        public IActionResult Create()
        {
            Compound compound = new Compound();
            PopulateAssignedCounselorData(compound);
            return View();
        }

        // POST: Compounds/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("ID,Name")] Compound compound, string[] selectedOptions)
        {
            try
            {
                UpdateCounselorCompounds(selectedOptions, compound);
                if (ModelState.IsValid)
                {
                    _context.Add(compound);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            PopulateAssignedCounselorData(compound);
            return View(compound);
        }

        // GET: Compounds/Edit/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compound = await _context.Compounds
                .Include(c=>c.CounselorCompounds)
                .AsNoTracking()
               .SingleOrDefaultAsync(d => d.ID == id);
            if (compound == null)
            {
                return NotFound();
            }
            PopulateAssignedCounselorData(compound);
            return View(compound);
        }

        // POST: Compounds/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions)
        {
            var compoundToUpdate = await _context.Compounds
                .Include(c => c.CounselorCompounds)
               .SingleOrDefaultAsync(d => d.ID == id);

            if (compoundToUpdate == null)
            {
                return NotFound();
            }

            //Update the Compound's Counselors
            UpdateCounselorCompounds(selectedOptions, compoundToUpdate);

            if (await TryUpdateModelAsync<Compound>(compoundToUpdate, "",
                p => p.Name))
            {
                try
                {
                    _context.Update(compoundToUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompoundExists(compoundToUpdate.ID))
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
            //Validaiton Error so give the user another chance.
            PopulateAssignedCounselorData(compoundToUpdate);
            return View(compoundToUpdate);
        }

        // GET: Compounds/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compound = await _context.Compounds
                .FirstOrDefaultAsync(m => m.ID == id);
            if (compound == null)
            {
                return NotFound();
            }

            return View(compound);
        }

        // POST: Compounds/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var compound = await _context.Compounds.FindAsync(id);
            try
            {
                _context.Compounds.Remove(compound);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.InnerException.Message.Contains("FK_Campers_Compounds_CompoundID"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, you cannot delete a compound with campers assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(compound);

        }

        private void PopulateAssignedCounselorData(Compound compound)
        {
            var allCounselors = _context.Counselors;
            var docCounselors = new HashSet<int>(compound.CounselorCompounds.Select(b => b.CounselorID));
            var selected = new List<OptionVM>();
            var available = new List<OptionVM>();
            foreach (var s in allCounselors)
            {
                if (docCounselors.Contains(s.ID))
                {
                    selected.Add(new OptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.FormalName
                    });
                }
                else
                {
                    available.Add(new OptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.FormalName
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateCounselorCompounds(string[] selectedOptions, Compound compoundToUpdate)
        {
            if (selectedOptions == null)
            {
                compoundToUpdate.CounselorCompounds = new List<CounselorCompound>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var compoundCounselors = new HashSet<int>(compoundToUpdate.CounselorCompounds.Select(b => b.CounselorID));
            foreach (var s in _context.Counselors)
            {
                if (selectedOptionsHS.Contains(s.ID.ToString()))
                {
                    if (!compoundCounselors.Contains(s.ID))
                    {
                        compoundToUpdate.CounselorCompounds.Add(new CounselorCompound
                        {
                            CounselorID = s.ID,
                            CompoundID = compoundToUpdate.ID
                        });
                    }
                }
                else
                {
                    if (compoundCounselors.Contains(s.ID))
                    {
                        CounselorCompound specToRemove = compoundToUpdate.CounselorCompounds.SingleOrDefault(d => d.CounselorID == s.ID);
                        _context.Remove(specToRemove);
                    }
                }
            }
        }

        private bool CompoundExists(int id)
        {
            return _context.Compounds.Any(e => e.ID == id);
        }
    }
}
