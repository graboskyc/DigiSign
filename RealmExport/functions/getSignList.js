exports = async function(){

  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Sign");
  
  var docs = await conn.find({}, {name:1, _id:1, type:1, order:1, feed:1}).sort({order:1, feed:1});
  
  return docs
  
};