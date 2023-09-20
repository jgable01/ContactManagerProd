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
    public class PersonController : Controller
    {
        private readonly ContactManagerContext _context;

        public PersonController(ContactManagerContext context)
        {
            _context = context;
        }

        // GET: People
        public async Task<IActionResult> Index()
        {
            return _context.Person != null ?
                        View(await _context.Person.ToListAsync()) :
                        Problem("Entity set 'ContactManagerContext.Person'  is null.");

        }

        // GET: People/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Person == null)
            {
                return NotFound();
            }

            // Eagerly loading the associated businesses and addresses
            Person? person = _context.Person
                                     .Include(p => p.BusinessPeople)
                                         .ThenInclude(bp => bp.Business)
                                     .Include(p => p.AddressPeople)
                                         .ThenInclude(ap => ap.Address)
                                     .SingleOrDefault(p => p.PersonID == id.Value);

            ViewData["AllBusinesses"] = await _context.Business.Where(b => !b.BusinessPeople.Any(bp => bp.PersonID == id.Value)).ToListAsync();
            ViewData["AllAddresses"] = await _context.Address.Where(a => !a.AddressPeople.Any(ap => ap.PersonID == id.Value)).ToListAsync();


            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // GET: People/Create
        public IActionResult Create(int? addressId, int? businessId)
        {
            ViewBag.AddressId = addressId;
            ViewBag.BusinessId = businessId;
            return View();
        }


        // POST: People/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PersonID,FirstName,LastName,Email,PhoneNumber")] Person person, int? AddressID, int? BusinessID)
        {
            if (ModelState.IsValid)
            {
                _context.Add(person);
                await _context.SaveChangesAsync();

                if (AddressID.HasValue)
                {
                    var addressPerson = new AddressPerson
                    {
                        PersonID = person.PersonID,
                        AddressID = AddressID.Value
                    };
                    _context.Add(addressPerson);
                    await _context.SaveChangesAsync();
                }

                if (BusinessID.HasValue)
                {
                    var businessPerson = new BusinessPerson
                    {
                        PersonID = person.PersonID,
                        BusinessID = BusinessID.Value
                    };
                    _context.Add(businessPerson);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(person);
        }



        // GET: People/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Person == null)
            {
                return NotFound();
            }

            var person = await _context.Person.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: People/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PersonID,FirstName,LastName,Email,PhoneNumber")] Person person)
        {
            if (id != person.PersonID)
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.PersonID))
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
            return View(person);
        }

        // GET: People/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null || _context.Person == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .FirstOrDefaultAsync(m => m.PersonID == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: People/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Person person = await _context.Person.FindAsync(id);
                if (person == null)
            {
                    return NotFound();
                }
    
                _context.Person.Remove(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssociateBusiness(int personId, int selectedBusinessId)
        {
            var person = await _context.Person.FindAsync(personId);
            var business = await _context.Business.FindAsync(selectedBusinessId);

            if (person == null || business == null)
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

            return RedirectToAction("Details", new { id = personId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DissociateBusiness(int PersonID, int selectedBusinessId)
        {
            Person person = _context.Person.Include(p => p.BusinessPeople)
                                           .FirstOrDefault(p => p.PersonID == PersonID);
            Business business = _context.Business.FirstOrDefault(b => b.BusinessID == selectedBusinessId);

            if (person == null || business == null)
            {
                return NotFound("Person or Business not found");
            }

            BusinessPerson? businessPerson = person.BusinessPeople
                                                   .FirstOrDefault(bp => bp.BusinessID == selectedBusinessId);

            if (businessPerson != null)
            {
                _context.Remove(businessPerson);  // Removing the BusinessPerson entity directly
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = PersonID });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssociateAddress(int personId, int selectedAddressId)
        {
            var person = await _context.Person.FindAsync(personId);
            var address = await _context.Address.FindAsync(selectedAddressId);

            if (person == null || address == null)
            {
                return NotFound();
            }

            var addressPerson = new AddressPerson
            {
                Address = address,
                Person = person
            };
            person.AddressPeople.Add(addressPerson);
            address.AddressPeople.Add(addressPerson);

            _context.Update(address);
            _context.Update(person);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = personId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DissociateAddress(int PersonID, int selectedAddressId)
        {
            Person person = _context.Person.Include(p => p.AddressPeople)
                                           .FirstOrDefault(p => p.PersonID == PersonID);
            Address address = _context.Address.FirstOrDefault(a => a.AddressID == selectedAddressId);

            if (person == null || address == null)
            {
                return NotFound("Person or Address not found");
            }

            AddressPerson? addressPerson = person.AddressPeople
                                                 .FirstOrDefault(ap => ap.AddressID == selectedAddressId);

            if (addressPerson != null)
            {
                _context.Remove(addressPerson);  // Removing the AddressPerson entity directly
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = PersonID });
        }


        private bool PersonExists(int id)
        {
            return (_context.Person?.Any(e => e.PersonID == id)).GetValueOrDefault();
        }
    }
}
