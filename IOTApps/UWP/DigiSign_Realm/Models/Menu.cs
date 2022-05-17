using System;
using System.Collections.Generic;
using System.Text;
using Realms;
using MongoDB.Bson;
using System.Globalization;

namespace DigiSign_Realm.Models
{
    class Menu : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        [Required]
        public string Id { get; set; }

        [MapTo("name")]
        [Required]
        public string Name { get; set; }

        [MapTo("soldAt")]
        [Required]
        public string SoldAtID { get; set; }

        [MapTo("soldAtFriendly")]
        public string SoldAtName { get; set; }

        [MapTo("catName")]
        public string CategoryName { get; set; }

        [MapTo("priceCents")]
        [Required]
        public double? PriceCents { get; set; } = 0.0;

        public string PriceFormatted {  get {
                string specifier = "C";
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                double p = PriceCents ?? 0.0;
                return (p / 100).ToString(specifier, culture);  
            } }
    }
}