using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampOrno.Data;
using CampOrno.Models;
using CampOrno.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using CampOrno.Utilities;

namespace CampOrno.Controllers
{
    [Authorize]
    public class CampersController : Controller
    {
        private readonly CampOrnoContext _context;

        public CampersController(CampOrnoContext context)
        {
            _context = context;
        }

        // GET: Campers
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Index(int? page,int? CompoundID, string SearchString, string AgeFrom, string AgeTo,
            int? DietaryRestrictionID, string actionButton, string sortDirection = "asc", string sortField = "Camper")
        {
            ViewData["DietaryRestrictionID"] = new SelectList(_context
                .DietaryRestrictions
                .OrderBy(c => c.Name), "ID", "Name");
            PopulateDropDownLists();
            ViewData["Filtering"] = "";  //Assume not filtering

            var campers = from c in _context.Campers
                .Include(c => c.Compound)
                .Include(c => c.Counselor)
                .Include(c => c.CamperDiets)
                .ThenInclude(cd => cd.DietaryRestriction)
                select c;

            //Add as many filters as needed
            if (CompoundID.HasValue)
            {
                campers = campers.Where(p => p.CompoundID == CompoundID);
                ViewData["Filtering"] = " show";
            }
            if (DietaryRestrictionID.HasValue)
            {
                campers = campers.Where(p => p.CamperDiets.Any(c => c.DietaryRestrictionID == DietaryRestrictionID));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                campers = campers.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            //Age Range Filter
            if(!String.IsNullOrEmpty(AgeFrom) || !String.IsNullOrEmpty(AgeTo))
            {
                //Give values we can work with if none supplied
                AgeFrom = String.IsNullOrEmpty(AgeFrom) ? "0" : AgeFrom;
                AgeTo = String.IsNullOrEmpty(AgeTo) ? "1000" : AgeTo;

                //Assume everything is good.
                ViewData["AgeRangeError"] = "";
                bool goodToTestAgeRange = true;
                int ageFrom;
                int ageTo;

                //Now test that everything is good
                if(!int.TryParse(AgeFrom, out ageFrom))
                {
                    ViewData["AgeRangeError"] = "-Invalid Age Mininum Value ";
                    goodToTestAgeRange = false;
                    ViewData["Filtering"] = " show";//We want to display the error message
                }
                if (!int.TryParse(AgeTo, out ageTo))
                {
                    ViewData["AgeRangeError"] += "-Invalid Age Maximum Value.";
                    goodToTestAgeRange = false;
                    ViewData["Filtering"] = " show";//We want to display the error message
                }

                //If everything still looks good then apply the filter.
                if(goodToTestAgeRange)
                {

                    if(ageFrom < ageTo)
                    {
                        DateTime dobFrom = DateTime.Today.AddYears(ageFrom * -1);
                        DateTime dobTo = DateTime.Today.AddYears(ageTo * -1);
                        campers = campers.Where(p => p.DOB <= dobFrom && p.DOB >= dobTo);
                    }
                    else
                    {
                        ViewData["AgeRangeError"] += "Maximum Age must be greater then the Miminum Age.";
                    }
                    ViewData["Filtering"] = " show";
                }
            }

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1;//Reset page to start
                if (actionButton != "Filter")//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by
            if (sortField == "Gender")
            {
                if (sortDirection == "asc")
                {
                    campers = campers
                        .OrderBy(p => p.Gender);
                }
                else
                {
                    campers = campers
                        .OrderByDescending(p => p.Gender);
                }
            }
            else if (sortField == "Age")
            {
                if (sortDirection == "asc")
                {
                    campers = campers
                        .OrderByDescending(p => p.DOB);
                }
                else
                {
                    campers = campers
                        .OrderBy(p => p.DOB);
                }
            }
            else if (sortField == "Compound")
            {
                if (sortDirection == "asc")
                {
                    campers = campers
                        .OrderBy(p => p.Compound.Name);
                }
                else
                {
                    campers = campers
                        .OrderByDescending(p => p.Compound.Name);
                }
            }
            else //Sorting by camper Name
            {
                if (sortDirection == "asc")
                {
                    campers = campers
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    campers = campers
                        .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize =10 ;//Change as required
            var pagedData = await PaginatedList<Camper>.CreateAsync(campers.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Campers/Details/5
        [Authorize(Roles="Staff, Supervisor, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var camper = await _context.Campers
                .Include(c => c.Compound)
                .Include(c => c.Counselor)
                .Include(c => c.CamperDiets)
                .ThenInclude(cd => cd.DietaryRestriction)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (camper == null)
            {
                return NotFound();
            }

            return View(camper);
        }

        // GET: Campers/Create
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public IActionResult Create()
        {
            var camper = new Camper();
            PopulateAssignedDietaryRestrictionData(camper);
            PopulateDropDownLists();
            return View();
        }

        // POST: Campers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,DOB,Gender,eMail,Phone,CounselorID,CompoundID")] Camper camper, string[] selectedOptions, IFormFile thePicture)
        {
            try
            {
                //Add the selected 
                if (selectedOptions != null)
                {
                    camper.CamperDiets = new List<CamperDiet>();
                    foreach (var r in selectedOptions)
                    {
                        var condToAdd = new CamperDiet { CamperID = camper.ID, DietaryRestrictionID = int.Parse(r) };
                        camper.CamperDiets.Add(condToAdd);
                    }
                }
                if (ModelState.IsValid)
                {
                    if (thePicture != null)
                    {
                        string mimeType = thePicture.ContentType;
                        long fileLength = thePicture.Length;
                        if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                        {
                            if (mimeType.Contains("image"))
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    await thePicture.CopyToAsync(memoryStream);
                                    camper.imageContent = memoryStream.ToArray();
                                }
                                camper.imageMimeType = mimeType;
                                camper.imageFileName = thePicture.FileName;
                            }
                        }
                        _context.Add(camper);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                if (dex.InnerException.Message.Contains("IX_Campers_eMail"))
                {
                    ModelState.AddModelError("eMail", "Unable to save changes. Remember, you cannot have duplicate eMail addresses.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }
            PopulateAssignedDietaryRestrictionData(camper);
            PopulateDropDownLists(camper);
            return View(camper);
        }

        // GET: Campers/Edit/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var camper = await _context.Campers
                .Include(c => c.Compound)
                .Include(c => c.Counselor)
                .Include(c => c.CamperDiets)
                .ThenInclude(cd => cd.DietaryRestriction)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID == id);
            if (camper == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(camper);
            PopulateAssignedDietaryRestrictionData(camper);
            return View(camper);
        }

        // POST: Campers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions, Byte[] RowVersion, string chkRemoveImage, IFormFile thePicture)
        {
            var camperToUpdate = await _context.Campers
                .Include(c => c.Compound)
                .Include(c => c.Counselor)
                .Include(c => c.CamperDiets)
                .ThenInclude(cd => cd.DietaryRestriction)
                .FirstOrDefaultAsync(c => c.ID == id);
            //Check that you got it or exit with a not found error
            if (camperToUpdate == null)
            {
                return NotFound();
            }

            //Update the medical history
            UpdateCamperDietaryRestrictions(selectedOptions, camperToUpdate);

            if (await TryUpdateModelAsync<Camper>(camperToUpdate, "",
                p => p.FirstName, p => p.MiddleName, p => p.LastName, p => p.DOB, p => p.Gender,
                p => p.CompoundID, p => p.Phone, p => p.eMail, p => p.CounselorID))
            {
                try
                {

                    //For the image
                    if (chkRemoveImage != null)
                    {
                        camperToUpdate.imageContent = null;
                        camperToUpdate.imageMimeType = null;
                        camperToUpdate.imageFileName = null;
                    }
                    else
                    {
                        if (thePicture != null)
                        {
                            string mimeType = thePicture.ContentType;
                            long fileLength = thePicture.Length;
                            if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                            {
                                if (mimeType.Contains("image"))
                                {
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        await thePicture.CopyToAsync(memoryStream);
                                        camperToUpdate.imageContent = memoryStream.ToArray();
                                    }
                                    camperToUpdate.imageMimeType = mimeType;
                                    camperToUpdate.imageFileName = thePicture.FileName;
                                }
                            }
                        }
                    }
                    //Put the original RowVersion value in the OriginalValues collection for the entity
                    _context.Entry(camperToUpdate).Property("RowVersion").OriginalValue = RowVersion;
                    _context.Update(camperToUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException ex)// Added for concurrency
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Camper)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The Camper was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Camper)databaseEntry.ToObject();
                        if (databaseValues.FirstName != clientValues.FirstName)
                            ModelState.AddModelError("FirstName", "Current value: "
                                + databaseValues.FirstName);
                        if (databaseValues.MiddleName != clientValues.MiddleName)
                            ModelState.AddModelError("MiddleName", "Current value: "
                                + databaseValues.MiddleName);
                        if (databaseValues.LastName != clientValues.LastName)
                            ModelState.AddModelError("LastName", "Current value: "
                                + databaseValues.LastName);
                        if (databaseValues.DOB != clientValues.DOB)
                            ModelState.AddModelError("DOB", "Current value: "
                                + String.Format("{0:d}", databaseValues.DOB));
                        if (databaseValues.Phone != clientValues.Phone)
                            ModelState.AddModelError("Phone", "Current value: "
                                + String.Format("{0:(###) ###-####}", databaseValues.Phone));
                        if (databaseValues.eMail != clientValues.eMail)
                            ModelState.AddModelError("eMail", "Current value: "
                                + databaseValues.eMail);
                        if (databaseValues.Gender != clientValues.Gender)
                            ModelState.AddModelError("Gender", "Current value: "
                                + databaseValues.Gender);
                        //For the foreign key, we need to go to the database to get the information to show
                        if (databaseValues.CompoundID != clientValues.CompoundID)
                        {
                            Compound databaseCompound = await _context.Compounds.SingleOrDefaultAsync(i => i.ID == databaseValues.CompoundID);
                            ModelState.AddModelError("CompoundID", $"Current value: {databaseCompound?.Name}");
                        }
                        //A little extra work for the nullable foreign key.  No sense going to the database and asking for something
                        //we already know is not there.
                        if (databaseValues.CounselorID != clientValues.CounselorID)
                        {
                            if (databaseValues.CounselorID.HasValue)
                            {
                                Counselor databaseCounselor = await _context.Counselors.SingleOrDefaultAsync(i => i.ID == databaseValues.CounselorID);
                                ModelState.AddModelError("CounselorID", $"Current value: {databaseCounselor?.FullName}");
                            }
                            else
                            {
                                ModelState.AddModelError("CounselorID", $"Current value: None");
                            }
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you received your values. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to save your version of this record, click "
                                + "the Save button again. Otherwise click the 'Back to List' hyperlink.");
                        camperToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.InnerException.Message.Contains("IX_Campers_eMail"))
                    {
                        ModelState.AddModelError("eMail", "Unable to save changes. Remember, you cannot have duplicate eMail addresses.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }

                }
            }
            PopulateDropDownLists(camperToUpdate);
            PopulateAssignedDietaryRestrictionData(camperToUpdate);
            return View(camperToUpdate);
        }

        // GET: Campers/Delete/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var camper = await _context.Campers
                .Include(c => c.Compound)
                .Include(c => c.Counselor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (camper == null)
            {
                return NotFound();
            }

            return View(camper);
        }

        // POST: Campers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var camper = await _context.Campers.FindAsync(id);
            try
            {
                _context.Campers.Remove(camper);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to delete camper.");
            }
            return View(camper);
        }

        private void PopulateAssignedDietaryRestrictionData(Camper camper)
        {
            var allDietaryRestrictions = _context.DietaryRestrictions;
            var camperDietaryRestrictions = new HashSet<int>(camper.CamperDiets.Select(b => b.DietaryRestrictionID));
            var checkBoxes = new List<AssignedOptionVM>();
            foreach (var dietaryRestriction in allDietaryRestrictions)
            {
                checkBoxes.Add(new AssignedOptionVM
                {
                    ID = dietaryRestriction.ID,
                    DisplayText = dietaryRestriction.Name,
                    Assigned = camperDietaryRestrictions.Contains(dietaryRestriction.ID)
                });
            }
            ViewData["DietaryRestrictions"] = checkBoxes;
        }

        private void UpdateCamperDietaryRestrictions(string[] selectedOptions, Camper camperToUpdate)
        {
            if (selectedOptions == null)
            {
                camperToUpdate.CamperDiets = new List<CamperDiet>();
                return;
            }

            var selectedDietaryRestrictionsHS = new HashSet<string>(selectedOptions);
            var camperDietaryRestrictionsHS = new HashSet<int>
                (camperToUpdate.CamperDiets.Select(c => c.DietaryRestrictionID));//IDs of the currently selected dietaryRestrictions
            foreach (var cond in _context.DietaryRestrictions)
            {
                if (selectedDietaryRestrictionsHS.Contains(cond.ID.ToString()))
                {
                    if (!camperDietaryRestrictionsHS.Contains(cond.ID))
                    {
                        camperToUpdate.CamperDiets.Add(new CamperDiet { CamperID = camperToUpdate.ID, DietaryRestrictionID = cond.ID });
                    }
                }
                else
                {
                    if (camperDietaryRestrictionsHS.Contains(cond.ID))
                    {
                        CamperDiet dietaryRestrictionToRemove = camperToUpdate.CamperDiets.SingleOrDefault(c => c.DietaryRestrictionID == cond.ID);
                        _context.Remove(dietaryRestrictionToRemove);
                    }
                }
            }
        }

        private SelectList CompoundSelectList(int? selectedId)
        {
            return new SelectList(_context.Compounds
                .OrderBy(c => c.Name), "ID", "Name", selectedId);
        }
        [HttpGet]
        public JsonResult GetCompounds(int? id)
        {
            return Json(CompoundSelectList(id));
        }
        private SelectList CounselorSelectList(int? selectedId)
        {
            //We will do this in two stages.  Get the data first and
            //then order it locally so we can use the FormalName to sort by.
            //Remember, the FormalName is not in the database so we can't sort by it unitl
            //we have the cousnelors in our own list.
            var counselors = _context.Counselors
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName).ToList();
            return new SelectList(counselors.OrderBy(c => c.FormalName), 
                "ID", "FormalName", selectedId);
        }
        [HttpGet]
        public JsonResult GetCounselors(int? id)
        {
            return Json(CounselorSelectList(id));
        }
        private void PopulateDropDownLists(Camper camper = null)
        {
            ViewData["CompoundID"] = CompoundSelectList(camper?.CompoundID);
            ViewData["CounselorID"] = CounselorSelectList(camper?.CounselorID);
        }

        private bool CamperExists(int id)
        {
            return _context.Campers.Any(e => e.ID == id);
        }
    }
}
