exports = async function(id){

  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Registration");
  
  var doc = await conn.findOne({_id:BSON.ObjectId(id)});
  
  return doc
  
};