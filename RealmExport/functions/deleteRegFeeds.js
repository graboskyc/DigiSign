exports = async function(id){
  //var obj = EJSON.parse(objstr);
  var now = new Date();
  
  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Registration");
  
  var id = BSON.ObjectId(id);
  var obj = {updated:now};
  
  await conn.updateOne({_id:id}, {$set:obj, $unset:{feed:""}})
  
};