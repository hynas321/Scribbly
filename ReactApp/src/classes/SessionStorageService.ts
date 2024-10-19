export class SessionStorageService {
  static authorizationTokenKey: string = "authorizationToken";
  static usernameKey: string = "username";
  static authOToken: string = "authOToken";
  static authOAccountId: string = "authOAccountId";

  private static instance: SessionStorageService;

  private constructor() {}

  static getInstance(): SessionStorageService {
    if (!SessionStorageService.instance) {
      SessionStorageService.instance = new SessionStorageService();
    }
    return SessionStorageService.instance;
  }

  setAuthorizationToken(value: string): void {
    sessionStorage.setItem(SessionStorageService.authorizationTokenKey, value);
  }

  getAuthorizationToken(): string {
    return sessionStorage.getItem(SessionStorageService.authorizationTokenKey) ?? "";
  }

  clearAuthorizationToken(): void {
    return sessionStorage.setItem(SessionStorageService.authorizationTokenKey, "");
  }

  setUsername(value: string): void {
    sessionStorage.setItem(SessionStorageService.usernameKey, value);
  }

  getUsername(): string {
    return sessionStorage.getItem(SessionStorageService.usernameKey) ?? "";
  }

  setAuthOToken(value: string): void {
    sessionStorage.setItem(SessionStorageService.authOToken, value);
  }

  getAuthOToken(): string {
    return sessionStorage.getItem(SessionStorageService.authOToken) ?? "";
  }

  setAuthOAccountId(value: string): void {
    sessionStorage.setItem(SessionStorageService.authOAccountId, value);
  }

  getAuthOAccountId(): string {
    return sessionStorage.getItem(SessionStorageService.authOAccountId) ?? "";
  }
}
