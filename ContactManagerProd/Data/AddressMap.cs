using System;
using System.Runtime.InteropServices;
using ContactManager.Models;
using CsvHelper.Configuration;

namespace ContactManager.Data

{
    public class AddressMap : ClassMap<Address> // Used to map the Address class to the CSVHelper library. I was having trouble handling empty fields, so I used the Optional() method to ignore them
    {
        public AddressMap()
        {
            Map(m => m.BusinessID).Optional().Name("BusinessID");
            Map(m => m.AddressID).Ignore();
            Map(m => m.StreetName).Name("StreetName");
            Map(m => m.StreetNumber).Name("StreetNumber");
            Map(m => m.UnitNumber).Optional().Name("UnitNumber");
            Map(m => m.PostalCode).Name("PostalCode");
        }
    }
}
