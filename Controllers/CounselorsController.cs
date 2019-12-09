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
using CampOrno.Utilities;

namespace CampOrno.Controllers
{
    [Authorize]
    public class CounselorsController : Controller
    {
        private readonly CampOrnoContext _context;

        public CounselorsController(CampOrnoContext context)
        {
            _context = context;
        }

        // GET: Counselors
        public async Task<IActionResult> Index(int? page)
        {
            var counselors = from c in _context.Counselors
                             .Include(c => c.Campers)
                             .Include(c => c.CounselorCompounds)
                             .ThenInclude(cc => cc.Compound)
                             select c;
            page = 1;//Reset page to start
           
            int pageSize = 10;//Change as required
            var pagedData = await PaginatedList<Counselor>.CreateAsync(counselors.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Counselors/Details/5
        [Authorize(Roles="Staff, Supervisor, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var counselor = await _context.Counselors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (counselor == null)
            {
                return NotFound();
            }

            return View(counselor);
        }

        // GET: Counselors/Create
        [Authorize(Roles = "Supervisor, Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Counselors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,Nickname,SIN")] Counselor counselor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(counselor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.InnerException.Message.Contains("IX_Counselors_SIN"))
                {
                    ModelState.AddModelError("SIN", "Unable to save changes. Remember, you cannot have duplicate SIN numbers.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(counselor);
        }

        // GET: Counselors/Edit/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var counselor = await _context.Counselors.FindAsync(id);
            if (counselor == null)
            {
                return NotFound();
            }
            return View(counselor);
        }

        // POST: Counselors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
            var counselorToUpdate = await _context.Counselors.FindAsync(id);
            //Check that you got it or exit with a not found error
            if (counselorToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Counselor>(counselorToUpdate, "",
                p => p.FirstName, p => p.MiddleName, p => p.LastName, p => p.Nickname,
                p => p.SIN))
            {
                try
                {
                    //Put the original RowVersion value in the OriginalValues collection for the entity
                    _context.Entry(counselorToUpdate).Property("RowVersion").OriginalValue = RowVersion;
                    _context.Update(counselorToUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)// Added for concurrency
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Counselor)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The Counselor was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Counselor)databaseEntry.ToObject();
                        if (databaseValues.FirstName != clientValues.FirstName)
                            ModelState.AddModelError("FirstName", "Current value: "
                                + databaseValues.FirstName);
                        if (databaseValues.MiddleName != clientValues.MiddleName)
                            ModelState.AddModelError("MiddleName", "Current value: "
                                + databaseValues.MiddleName);
                        if (databaseValues.LastName != clientValues.LastName)
                            ModelState.AddModelError("LastName", "Current value: "
                                + databaseValues.LastName);
                        if (databaseValues.Nickname != clientValues.Nickname)
                            ModelState.AddModelError("Nickname", "Current value: "
                                + databaseValues.Nickname);
                        if (databaseValues.SIN != clientValues.SIN)
                            ModelState.AddModelError("SIN", "Current value: "
                                + databaseValues.SIN);
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you received your values. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to save your version of this record, click "
                                + "the Save button again. Otherwise click the 'Back to List' hyperlink.");
                        counselorToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.InnerException.Message.Contains("IX_Counselors_SIN"))
                    {
                        ModelState.AddModelError("SIN", "Unable to save changes. Remember, you cannot have duplicate SIN numbers.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            return View(counselorToUpdate);
        }

        // GET: Counselors/Delete/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var counselor = await _context.Counselors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (counselor == null)
            {
                return NotFound();
            }

            return View(counselor);
        }

        // POST: Counselors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var counselor = await _context.Counselors.FindAsync(id);
            try
            {
                _context.Counselors.Remove(counselor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.InnerException.Message.Contains("FK_CounselorCompounds_Counselors_CounselorID"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, you cannot delete a counselor assigned to any compounds.");
                }
                else if (dex.InnerException.Message.Contains("FK_Campers_Counselors_CounselorID"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, you cannot delete a Lead Counselor assigned to a camper as a mentor.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(counselor);
        }

        private bool CounselorExists(int id)
        {
            return _context.Counselors.Any(e => e.ID == id);
        }
    }
}
