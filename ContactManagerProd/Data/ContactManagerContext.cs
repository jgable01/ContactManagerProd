using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using ContactManager.Models;
using System.Globalization;
using System.Collections.Generic;

namespace ContactManager.Data
{
    public class ContactManagerContext : DbContext
    {
        public ContactManagerContext (DbContextOptions<ContactManagerContext> options)
            : base(options)
        {
        }

        public DbSet<ContactManager.Models.Person> Person { get; set; } = default!;

        public DbSet<ContactManager.Models.Address> Address { get; set; } = default!;

        public DbSet<ContactManager.Models.Business> Business { get; set; } = default!;

        public void Seed(ContactManagerContext context)
        {

            // Clear existing data
            context.Address.RemoveRange(context.Address);
            context.Business.RemoveRange(context.Business);
            context.Person.RemoveRange(context.Person);

            // start ids at 1
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT('[Address]', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT('[Business]', RESEED, 0)");
            context.Database.ExecuteSqlRaw("DBCC CHECKIDENT('[Person]', RESEED, 0)");
            context.SaveChanges();

            // Ensure database is created
            context.Database.EnsureCreated();

            // Configure CSV Reader to ignore missing fields
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null,
            };

            // Seed Persons from CSV
            using (var reader = new StreamReader("DataFiles/Addresses.csv"))
            using (var csv = new CsvReader(reader, config))
            {
                // Read all records
                var persons = csv.GetRecords<Person>().ToList();
                context.Person.AddRange(persons);
            }

            // Seed Businesses from CSV
            using (var reader = new StreamReader("DataFiles/Businesses.csv"))
            using (var csv = new CsvReader(reader, config))
            {
                var businesses = csv.GetRecords<Business>().ToList();
                context.Business.AddRange(businesses);
            }
            context.SaveChanges();


            // Seed Addresses from CSV
            using (var reader = new StreamReader("DataFiles/Persons.csv"))
            using (var csv = new CsvReader(reader, config))
            {   
                csv.Context.RegisterClassMap<AddressMap>();
                var addresses = csv.GetRecords<Address>().ToList();
                // assign the addresses to the businesses
                foreach (var address in addresses)
                {
                    if (address.BusinessID != null)
                    {
                        address.Business = context.Business.Find(address.BusinessID);
                    } 
                }
                context.Address.AddRange(addresses);
            }

            // Save changes to the database
            context.SaveChanges();



        }

    }

}
