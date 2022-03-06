using System;
using System.Collections.Generic;
using System.Text;
using Realms;
using MongoDB.Bson;

namespace DigiSign_Realm.Models
{
    class Sponsor : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [MapTo("name")]
        public string Name { get; set; } 

        [MapTo("text")]
        public string Text { get; set; } 

        [MapTo("type")]
        public string Type { get; set; }

    }
}
