import express from 'express';
import { createProxyMiddleware } from 'http-proxy-middleware';
import { spawn } from 'child_process';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const app = express();
const PORT = 5000;

console.log('🚀 Starting DSAGrind Proxy Server...');

// Start the .NET backend
const startDotNetBackend = () => {
  console.log('🔧 Starting .NET microservices backend...');
  const dotnetProcess = spawn('bash', ['start-dotnet.sh'], {
    cwd: path.join(__dirname, '..'),
    stdio: 'inherit'
  });

  dotnetProcess.on('error', (error) => {
    console.error('❌ Failed to start .NET backend:', error);
  });

  dotnetProcess.on('exit', (code) => {
    console.log(`🔄 .NET backend exited with code ${code}`);
  });

  return dotnetProcess;
};

// Start .NET backend
const dotnetProcess = startDotNetBackend();

// Health check endpoint for Replit
app.get('/health', (req, res) => {
  res.json({ 
    status: 'healthy', 
    service: 'DSAGrind Proxy', 
    backend: '.NET 8 Microservices',
    timestamp: new Date().toISOString() 
  });
});

// Proxy all API requests to .NET Gateway
app.use('/api', createProxyMiddleware({
  target: 'http://localhost:8000',
  changeOrigin: true
}));

// Serve static files from client build
app.use(express.static(path.join(__dirname, '../dist/public')));

// Handle client-side routing - send all non-API requests to index.html
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, '../dist/public/index.html'));
});

app.listen(PORT, '0.0.0.0', () => {
  console.log(`✅ DSAGrind Proxy Server running on http://0.0.0.0:${PORT}`);
  console.log(`🌐 Frontend: http://localhost:${PORT}`);
  console.log(`🔧 .NET Backend: Microservices architecture`);
  console.log(`📚 API Gateway: http://localhost:5000/api`);
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('🛑 Shutting down proxy server...');
  if (dotnetProcess) {
    dotnetProcess.kill('SIGTERM');
  }
  process.exit(0);
});