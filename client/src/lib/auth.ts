import { apiRequest } from './queryClient';

export class AuthService {
  static getToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  static isAuthenticated(): boolean {
    return !!this.getToken();
  }

  static async refreshToken(): Promise<void> {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await apiRequest('POST', '/api/auth/refresh', { refreshToken });
    const data = await response.json();
    
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
  }

  static logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    window.location.href = '/';
  }
}

// Interceptor for automatic token refresh
const originalApiRequest = apiRequest;

export async function apiRequestWithAuth(
  method: string,
  url: string,
  data?: unknown
): Promise<Response> {
  try {
    return await originalApiRequest(method, url, data);
  } catch (error: any) {
    if (error.message.includes('401') && AuthService.isAuthenticated()) {
      try {
        await AuthService.refreshToken();
        return await originalApiRequest(method, url, data);
      } catch (refreshError) {
        AuthService.logout();
        throw refreshError;
      }
    }
    throw error;
  }
}
