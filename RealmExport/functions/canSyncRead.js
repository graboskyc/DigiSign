exports = function(p){
  console.log(p);
  const pklist = p.split(",");
  var u = context.user;
  
  var retval = false;
  
  if(pklist.indexOf("ALL")> -1) { retval = true; }
  else if(pklist.indexOf(u.custom_data.group)> -1) { retval = true; }
  
  return retval;
};