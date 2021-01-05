function getIPAddress() {
    var interfaces = require('os').networkInterfaces();
    for (var devName in interfaces) {
      var iface = interfaces[devName];
  
      for (var i = 0; i < iface.length; i++) {
        var alias = iface[i];
        if (
            alias.family === 'IPv4' && 
            alias.address !== '127.0.0.1' && 
            !alias.internal &&
            !alias.address.includes('192.168.') &&
            !alias.address.includes('0.0.0.0')
            )
          return alias.address;
      }
    }
    return '0.0.0.0';
  }

  const http = require('http');

  const hostname = getIPAddress();
  const port = 4999;
  var ips = [];
  
  const server = http.createServer((request, response) => {
    const { headers, method, url } = request;

    request.on('error', (err) => {
        console.error("Error: " + err);
        response.statusCode = 403;
        response.end();
    });
    if (request.method === 'GET' && (request.url === '/get')) {
        response.body = JSON.stringify(ips);
        response.statusCode = 200;
        response.end();
    }
    if (request.method === 'POST' && (request.url === '/register')) {
        
        response.statusCode = 200;
        response.end();
    }
  });
  
  server.listen(port, hostname, () => {
    console.log(`Server running at http://${hostname}:${port}/`);
  });