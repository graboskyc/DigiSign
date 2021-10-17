exports = async function(id){

  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Sign");
  
  await conn.deleteOne({_id:BSON.ObjectId(id)});
  
};