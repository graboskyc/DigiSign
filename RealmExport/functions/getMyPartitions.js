exports = async function(deviceID){
  retval = "";
  
  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Registration");
  
  var doc = await conn.findOne({deviceId:deviceID});
  
  if(doc) {
    if(doc.hasOwnProperty("feed")) {
      retval = doc.feed;
    }
  } else {
    await conn.insertOne({deviceId:deviceID});
  }
  
  return retval;
};