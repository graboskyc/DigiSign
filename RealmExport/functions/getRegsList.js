exports = async function(){

  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Registration");
  
  var docs = await conn.find({});
  
  return docs
  
};