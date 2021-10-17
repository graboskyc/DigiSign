exports = async function(obj){
  //var obj = EJSON.parse(objstr);
  var now = new Date();
  
  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Sign");
  
  var id = BSON.ObjectId(obj._id);
  delete obj._id;
  obj.updated = now;
  obj.duration = obj.duration*1;
  obj.order = obj.order*1;
  obj._pk = "GLOBAL";
  
  await conn.updateOne({_id:id}, {$set:obj})
  
};