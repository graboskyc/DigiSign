exports = async function(obj){
  
  var now = new Date();
  
  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Sign");
  
  delete obj._id;
  obj.updated = now;
  obj.created = now;
  obj.duration = obj.duration*1;
  obj.order = obj.order*1;
  obj._pk = "GLOBAL";
  
  await conn.insertOne(obj);
  
};