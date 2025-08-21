import jwt from "jsonwebtoken";

const JWT_SECRET = process.env.JWT_SECRET || "dev_secret_key_change_in_production";

// Email service interface
export interface IEmailService {
  sendVerificationEmail(email: string, token: string): Promise<boolean>;
  sendPasswordResetEmail(email: string, token: string): Promise<boolean>;
  sendWelcomeEmail(email: string, name: string): Promise<boolean>;
}

// Mock email service for development
export class MockEmailService implements IEmailService {
  async sendVerificationEmail(email: string, token: string): Promise<boolean> {
    const verificationUrl = `${process.env.FRONTEND_URL || 'http://localhost:5000'}/verify-email?token=${token}`;
    
    console.log(`📧 Email verification sent to: ${email}`);
    console.log(`🔗 Verification URL: ${verificationUrl}`);
    console.log(`🎫 Token: ${token}`);
    
    return true;
  }

  async sendPasswordResetEmail(email: string, token: string): Promise<boolean> {
    const resetUrl = `${process.env.FRONTEND_URL || 'http://localhost:5000'}/reset-password?token=${token}`;
    
    console.log(`🔑 Password reset sent to: ${email}`);
    console.log(`🔗 Reset URL: ${resetUrl}`);
    console.log(`🎫 Token: ${token}`);
    
    return true;
  }

  async sendWelcomeEmail(email: string, name: string): Promise<boolean> {
    console.log(`👋 Welcome email sent to: ${email} (${name})`);
    return true;
  }
}

export const emailService = new MockEmailService();

// Email verification utilities
export function generateVerificationToken(email: string): string {
  return jwt.sign({ email, type: 'email_verification' }, JWT_SECRET, { expiresIn: '24h' });
}

export function generatePasswordResetToken(email: string): string {
  return jwt.sign({ email, type: 'password_reset' }, JWT_SECRET, { expiresIn: '1h' });
}

export function verifyToken(token: string): { email: string; type: string } | null {
  try {
    const decoded = jwt.verify(token, JWT_SECRET) as any;
    return { email: decoded.email, type: decoded.type };
  } catch (error) {
    return null;
  }
}