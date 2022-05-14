using System;
using System.Collections.Generic;
using System.Text;
using Realms;
using MongoDB.Bson;


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

        public string PriceFormatted {  get { return "$" + (PriceCents / 100).ToString();  } }
    }
}