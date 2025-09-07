// Simple Node.js backend to demonstrate API integration while .NET services build
const http = require('http');
const url = require('url');

const server = http.createServer((req, res) => {
  // Set CORS headers for frontend integration
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization');

  if (req.method === 'OPTIONS') {
    res.writeHead(200);
    res.end();
    return;
  }

  const parsedUrl = url.parse(req.url, true);
  const path = parsedUrl.pathname;

  // Handle health check
  if (path === '/health' || path === '/api/health') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ 
      status: 'healthy', 
      message: 'Simple backend running - .NET services building',
      timestamp: new Date().toISOString()
    }));
    return;
  }

  // Handle auth endpoints
  if (path === '/api/auth/register' && req.method === 'POST') {
    let body = '';
    req.on('data', chunk => body += chunk);
    req.on('end', () => {
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ 
        success: true, 
        message: 'Registration will be available when .NET Auth service starts',
        userId: 'demo-user-' + Date.now()
      }));
    });
    return;
  }

  if (path === '/api/auth/login' && req.method === 'POST') {
    let body = '';
    req.on('data', chunk => body += chunk);
    req.on('end', () => {
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ 
        success: true, 
        message: 'Login will be available when .NET Auth service starts',
        token: 'demo-jwt-token',
        user: { id: 'demo-user', email: 'test@example.com', role: 'user' }
      }));
    });
    return;
  }

  // Default response
  res.writeHead(404, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify({ 
    error: 'Endpoint not found', 
    message: '.NET services are building. This is a temporary demo backend.' 
  }));
});

const PORT = 8001;
server.listen(PORT, '0.0.0.0', () => {
  console.log(`Demo backend running on http://0.0.0.0:${PORT}`);
  console.log('This will be replaced by .NET Gateway when build completes');
});