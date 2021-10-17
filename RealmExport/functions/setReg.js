exports = async function(obj){
  //var obj = EJSON.parse(objstr);
  var now = new Date();
  
  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Registration");
  
  var id = BSON.ObjectId(obj._id);
  delete obj._id;
  delete obj.deviceId;
  obj.updated = now;
  
  await conn.updateOne({_id:id}, {$set:obj})
  
};