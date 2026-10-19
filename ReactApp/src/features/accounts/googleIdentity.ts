interface GoogleIdentity {
  initialize(options: {
    client_id: string;
    callback: (response: { credential: string }) => void;
  }): void;
  renderButton(element: HTMLElement, options: { theme: string; size: string }): void;
  disableAutoSelect(): void;
}

declare global {
  interface Window {
    google?: { accounts: { id: GoogleIdentity } };
  }
}

let loading: Promise<GoogleIdentity> | undefined;

export function loadGoogleIdentity(): Promise<GoogleIdentity> {
  if (window.google?.accounts.id) {
    return Promise.resolve(window.google.accounts.id);
  }
  if (loading) {
    return loading;
  }
  loading = new Promise<GoogleIdentity>((resolve, reject) => {
    const script = document.createElement("script");
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    const timeout = window.setTimeout(() => {
      script.onload = null;
      script.onerror = null;
      script.remove();
      reject(new Error("Google sign-in timed out"));
    }, 15000);
    script.onload = () => {
      window.clearTimeout(timeout);
      if (window.google?.accounts.id) {
        resolve(window.google.accounts.id);
      } else {
        script.remove();
        reject(new Error("Google sign-in did not initialize"));
      }
    };
    script.onerror = () => {
      window.clearTimeout(timeout);
      script.remove();
      reject(new Error("Google sign-in could not load"));
    };
    document.head.append(script);
  }).catch((error) => {
    loading = undefined;
    throw error;
  });
  return loading;
}
