exports = async function(id){
 
  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Registration");
  
  var id = BSON.ObjectId(id);
  
  await conn.deleteOne({_id:id});
  
};