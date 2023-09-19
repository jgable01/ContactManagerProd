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
    public class AddressController : Controller
    {
        private readonly ContactManagerContext _context;

        public AddressController(ContactManagerContext context)
        {
            _context = context;
        }

        // GET: Addresses
        public async Task<IActionResult> Index()
        {
            // Populate the view with the addresses for dropdown selection
              return _context.Address != null ? 
                          View(await _context.Address.ToListAsync()) :
                          Problem("Entity set 'ContactManagerContext.Address'  is null.");
        }

        // GET: Address/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Address == null)
            {
                return NotFound();
            }

            ViewData["AllPersons"] = _context.Person.ToList();
            ViewData["AllBusinesses"] = _context.Business.ToList();

            // Eagerly loading the associated people and business
            Address? address = await _context.Address
                                    .Include(a => a.AddressPeople)
                                        .ThenInclude(ap => ap.Person)
                                    .Include(a => a.Business)
                                    .FirstOrDefaultAsync(a => a.AddressID == id.Value);

            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // GET: Addresses/Create
        // GET: Addresses/Create
        public IActionResult Create(int? businessId, int? personId)
        {
            ViewBag.BusinessId = businessId;
            ViewBag.PersonId = personId;
            return View();
        }


        // POST: Addresses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StreetName,StreetNumber,UnitNumber,PostalCode")] Address address, int? businessId, int? personId)
        {
            if (ModelState.IsValid)
            {
                _context.Add(address);
                await _context.SaveChangesAsync();  // Save the address first to get a permanent ID

                bool isUpdated = false; // Used to track if an update operation is required

                if (businessId.HasValue)
                {
                    address.BusinessID = businessId.Value;
                    isUpdated = true;
                }

                if (personId.HasValue)
                {
                    var addressPerson = new AddressPerson
                    {
                        PersonID = personId.Value,
                        AddressID = address.AddressID
                    };
                    _context.Add(addressPerson);
                    isUpdated = true;
                }

                // Only call SaveChanges again if there were updates or associations
                if (isUpdated)
                {
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }




        // GET: Addresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Address == null)
            {
                return NotFound();
            }

            var address = await _context.Address.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }
            return View(address);
        }

        // POST: Addresses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AddressID,StreetName,StreetNumber,UnitNumber,PostalCode")] Address address)
        {
            if (id != address.AddressID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(address);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddressExists(address.AddressID))
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
            return View(address);
        }

        // GET: Addresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Address == null)
            {
                return NotFound();
            }

            var address = await _context.Address
                .FirstOrDefaultAsync(m => m.AddressID == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // POST: Addresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Address == null)
            {
                return Problem("Entity set 'ContactManagerContext.Address'  is null.");
            }
            var address = await _context.Address.FindAsync(id);
            if (address != null)
            {
                _context.Address.Remove(address);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AssociatePerson(int AddressID, int selectedPersonId)
        {
            // Fetch the address and person based on the IDs
            Address address = _context.Address.Find(AddressID);
            Person person = _context.Person.Find(selectedPersonId);

            if (address == null || person == null)
            {
                return NotFound("Address or Person not found");
            }

            // Check if they are not already associated
            if (!address.AddressPeople.Any(ap => ap.PersonID == selectedPersonId))
            {
                // Create the association
                AddressPerson addressPerson = new AddressPerson
                {
                    Address = address,
                    Person = person
                };
                address.AddressPeople.Add(addressPerson);
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = AddressID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DissociatePerson(int AddressID, int selectedPersonId)
        {
            Address address = _context.Address.Include(a => a.AddressPeople)
                                                .FirstOrDefault(a => a.AddressID == AddressID);
            Person person = _context.Person.FirstOrDefault(p => p.PersonID == selectedPersonId);

            if (address == null || person == null)
            {
                return NotFound("Address or Person not found");
            }

            AddressPerson? addressPerson = address.AddressPeople
                                                  .FirstOrDefault(ap => ap.PersonID == selectedPersonId);

            if (addressPerson != null)
            {
                _context.Remove(addressPerson);  // Removing the AddressPerson entity directly
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = AddressID });
        }

        [HttpPost]
        public IActionResult AssociateBusiness(int AddressID, int selectedBusinessId)
        {
            // Fetch the address and business based on the IDs
            Address address = _context.Address.Find(AddressID);
            Business business = _context.Business.Find(selectedBusinessId);

            if (address == null || business == null)
            {
                return NotFound("Address or Business not found");
            }

            // Check if they are not already associated
            if (address.BusinessID != selectedBusinessId)
            {
                // Create the association
                address.Business = business;
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = AddressID });
        }

        [HttpPost]
        public IActionResult DissociateBusiness(int AddressID)
        {
            Address address = _context.Address.Include(a => a.Business)
                                                .FirstOrDefault(a => a.AddressID == AddressID);

            if (address == null)
            {
                return NotFound("Address not found");
            }

            if (address.Business != null)
            {
                address.Business = null;
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = AddressID });
        }

        private bool AddressExists(int id)
        {
          return (_context.Address?.Any(e => e.AddressID == id)).GetValueOrDefault();
        }
    }
}
