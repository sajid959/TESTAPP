import { WebSocketServer, WebSocket } from 'ws';
import { Server } from 'http';
import { AuthService } from './auth';
import { storage } from '../storage';

interface WebSocketClient extends WebSocket {
  userId?: string;
  isAlive?: boolean;
}

export class WebSocketService {
  private wss: WebSocketServer;
  private clients = new Map<string, WebSocketClient>();

  constructor(server: Server) {
    this.wss = new WebSocketServer({ 
      server, 
      path: '/ws',
      verifyClient: async (info) => {
        // Optional: Verify JWT token from query params or headers
        return true;
      }
    });

    this.setupWebSocketServer();
    this.setupHeartbeat();
  }

  private setupWebSocketServer() {
    this.wss.on('connection', async (ws: WebSocketClient, req) => {
      console.log('WebSocket connection established');

      // Extract token from query params for authentication
      const url = new URL(req.url!, `http://${req.headers.host}`);
      const token = url.searchParams.get('token');

      if (token) {
        const payload = AuthService.verifyAccessToken(token);
        if (payload) {
          ws.userId = payload.userId;
          this.clients.set(payload.userId, ws);
        }
      }

      ws.isAlive = true;

      ws.on('pong', () => {
        ws.isAlive = true;
      });

      ws.on('message', async (data) => {
        try {
          const message = JSON.parse(data.toString());
          await this.handleMessage(ws, message);
        } catch (error) {
          console.error('Error parsing WebSocket message:', error);
        }
      });

      ws.on('close', () => {
        if (ws.userId) {
          this.clients.delete(ws.userId);
        }
        console.log('WebSocket connection closed');
      });

      // Send welcome message
      ws.send(JSON.stringify({
        type: 'connected',
        message: 'WebSocket connection established'
      }));
    });
  }

  private async handleMessage(ws: WebSocketClient, message: any) {
    const { type, data } = message;

    switch (type) {
      case 'ping':
        ws.send(JSON.stringify({ type: 'pong' }));
        break;

      case 'join_problem':
        if (ws.userId && data.problemId) {
          // Join problem room for real-time collaboration
          ws.send(JSON.stringify({
            type: 'joined_problem',
            data: { problemId: data.problemId }
          }));
        }
        break;

      case 'code_update':
        if (ws.userId && data.problemId && data.code) {
          // Broadcast code updates to other users working on same problem
          this.broadcastToProblemRoom(data.problemId, {
            type: 'code_updated',
            data: {
              userId: ws.userId,
              code: data.code,
              language: data.language
            }
          }, ws.userId);
        }
        break;

      case 'submission_status':
        // Real-time submission status updates are handled by the submission service
        break;

      default:
        console.log('Unknown message type:', type);
    }
  }

  private setupHeartbeat() {
    setInterval(() => {
      this.wss.clients.forEach((ws: WebSocketClient) => {
        if (!ws.isAlive) {
          ws.terminate();
          if (ws.userId) {
            this.clients.delete(ws.userId);
          }
          return;
        }
        ws.isAlive = false;
        ws.ping();
      });
    }, 30000);
  }

  // Public methods for sending notifications
  public sendToUser(userId: string, message: any) {
    const client = this.clients.get(userId);
    if (client && client.readyState === WebSocket.OPEN) {
      client.send(JSON.stringify(message));
    }
  }

  public broadcastToProblemRoom(problemId: string, message: any, excludeUserId?: string) {
    this.clients.forEach((client, userId) => {
      if (userId !== excludeUserId && client.readyState === WebSocket.OPEN) {
        client.send(JSON.stringify({
          ...message,
          problemId
        }));
      }
    });
  }

  public broadcastToAll(message: any) {
    this.clients.forEach((client) => {
      if (client.readyState === WebSocket.OPEN) {
        client.send(JSON.stringify(message));
      }
    });
  }

  public notifySubmissionUpdate(userId: string, submission: any) {
    this.sendToUser(userId, {
      type: 'submission_update',
      data: submission
    });
  }

  public notifyNewDiscussion(problemId: string, discussion: any) {
    this.broadcastToProblemRoom(problemId, {
      type: 'new_discussion',
      data: discussion
    });
  }
}
