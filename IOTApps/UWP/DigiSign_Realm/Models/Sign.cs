using System;
using System.Collections.Generic;
using System.Text;
using Realms;
using MongoDB.Bson;

namespace DigiSign_Realm.Models
{
    class Sign : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [MapTo("order")]
        public int Order { get; set; } = 0;

        [MapTo("duration")]
        public int Duration { get; set; } = 0;

        [MapTo("type")]
        [Required]
        public string Type { get; set; }

        [MapTo("feed")]
        [Required]
        public string Feed { get; set; }

        [MapTo("name")]
        [Required]
        public string Name { get; set; }

        [MapTo("text")]
        public string Text { get; set; }

        [MapTo("uri")]
        public string URI { get; set; }
        [MapTo("format")]
        public string TextFormat { get; set; }

    }
}
