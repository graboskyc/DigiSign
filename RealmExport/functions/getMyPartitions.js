exports = async function(deviceID, ip){
  retval = "";
  var now = new Date();
  
  var conn = context.services.get("mongodb-atlas").db("digisign_realm").collection("Registration");
  
  var doc = await conn.findOne({deviceId:deviceID});
  
  if(doc) {
    if(doc.hasOwnProperty("feed")) {
      retval = doc.feed;
      await conn.updateOne({deviceId:deviceID}, {$set:{lastSeen:now, ipaddr:ip}});
    }
  } else {
    await conn.insertOne({deviceId:deviceID, firstSeen:now, lastSeen:now, ipaddr:ip});
  }
  
  return retval;
};