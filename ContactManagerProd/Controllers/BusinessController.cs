using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactManager.Data;
using ContactManager.Models;

namespace ContactManager.Controllers
{
    public class BusinessController : Controller
    {
        private readonly ContactManagerContext _context;

        public BusinessController(ContactManagerContext context)
        {
            _context = context;
        }

        // GET: Businesses
        public async Task<IActionResult> Index()
        {
            return _context.Business != null ?
                        View(await _context.Business.ToListAsync()) :
                        Problem("Entity set 'ContactManagerContext.Business'  is null.");
        }

        // GET: Businesses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // populate the view with businesses for dropdown selection
            if (id == null || _context.Business == null)
            {
                return NotFound();
            }

            // Eagerly loading the associated people and addresses
            Business? business = await _context.Business
                                    .Include(b => b.BusinessPeople)
                                        .ThenInclude(bp => bp.Person)
                                    .Include(b => b.Addresses)
                                    .FirstOrDefaultAsync(b => b.BusinessID == id.Value);

            ViewData["AllAddresses"] = await _context.Address.Where(a => a.BusinessID == null).ToListAsync();
            ViewData["AllPersons"] = await _context.Person.ToListAsync();

            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // GET: Businesses/Create
        public IActionResult Create(int? personId)
        {
            ViewBag.PersonId = personId;
            ViewBag.AddressId = new SelectList(_context.Address.Where(a => a.BusinessID == null), "AddressID", "FullAddress");
            return View();
        }


        // POST: Businesses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusinessID,BusinessName,PhoneNumber,Email")] Business business, int? PersonID)
        {
            if (ModelState.IsValid)
            {
                _context.Add(business);
                await _context.SaveChangesAsync();

                if (PersonID.HasValue)
                {
                    var businessPerson = new BusinessPerson
                    {
                        BusinessID = business.BusinessID,
                        PersonID = PersonID.Value
                    };
                    _context.Add(businessPerson);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(business);
        }



        // GET: Businesses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Business == null)
            {
                return NotFound();
            }

            var business = await _context.Business.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }
            return View(business);
        }

        // POST: Businesses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BusinessID,BusinessName,PhoneNumber,Email")] Business business)
        {
            if (id != business.BusinessID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(business);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessExists(business.BusinessID))
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
            return View(business);
        }

        // GET: Businesses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Business == null)
            {
                return NotFound();
            }

            var business = await _context.Business
                .FirstOrDefaultAsync(m => m.BusinessID == id);
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // POST: Businesses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var business = await _context.Business.FindAsync(id);

            if (business == null)
            {
                return NotFound();
            }

            // Remove any associated addresses for the business
            var associatedAddresses = _context.Address.Where(a => a.BusinessID == id);
            foreach (var address in associatedAddresses)
            {
                address.BusinessID = null;
            }

            _context.Business.Remove(business);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssociateAddress(int businessId, int selectedAddressId)
        {
            var business = await _context.Business.FindAsync(businessId);
            var address = await _context.Address.FindAsync(selectedAddressId);

            if (business == null || address == null)
            {
                return NotFound();
            }

            address.BusinessID = businessId;
            _context.Update(address);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = businessId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DissociateAddress(int BusinessID, int selectedAddressId)
        {
            Business business = _context.Business.Include(b => b.Addresses)
                                                 .FirstOrDefault(b => b.BusinessID == BusinessID);
            Address address = _context.Address.FirstOrDefault(a => a.AddressID == selectedAddressId);

            if (business == null || address == null)
            {
                return NotFound("Business or Address not found");
            }

            // Assuming each business can have multiple addresses, we might need to modify this logic
            if (address.BusinessID == BusinessID)
            {
                address.BusinessID = null;
                _context.Update(address);
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = BusinessID });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssociatePerson(int businessId, int selectedPersonId)
        {
            var business = await _context.Business.FindAsync(businessId);
            var person = await _context.Person.FindAsync(selectedPersonId);

            if (business == null || person == null)
            {
                return NotFound();
            }

            var businessPerson = new BusinessPerson
            {
                Business = business,
                Person = person
            };
            business.BusinessPeople.Add(businessPerson);
            person.BusinessPeople.Add(businessPerson);

            _context.Update(business);
            _context.Update(person);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = businessId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DissociatePerson(int BusinessID, int selectedPersonId)
        {
            Business business = _context.Business.Include(b => b.BusinessPeople)
                                                 .FirstOrDefault(b => b.BusinessID == BusinessID);
            Person person = _context.Person.FirstOrDefault(p => p.PersonID == selectedPersonId);

            if (business == null || person == null)
            {
                return NotFound("Business or Person not found");
            }

            BusinessPerson? businessPerson = business.BusinessPeople
                                                     .FirstOrDefault(bp => bp.PersonID == selectedPersonId);

            if (businessPerson != null)
            {
                _context.Remove(businessPerson);  // Removing the BusinessPerson entity directly
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = BusinessID });
        }

        private bool BusinessExists(int id)
        {
            return (_context.Business?.Any(e => e.BusinessID == id)).GetValueOrDefault();
        }
    }
}
