import { writable } from 'svelte/store';
import { goto } from '$app/navigation';
import { API_URL } from '$lib';
import type { LoginRequest, UserInfoResponse, AuthState } from '$lib/models/identity';

class AuthService {
    private static readonly BASE_REQUEST_CONFIG = {
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include' as RequestCredentials
    };

    static async login(loginRequest: LoginRequest): Promise<boolean> {
        try {
            const response = await fetch(`${API_URL}/account/login`, {
                ...this.BASE_REQUEST_CONFIG,
                method: 'POST',
                body: JSON.stringify(loginRequest)
            });

            return response.ok;
        } catch {
            return false;
        }
    }

    static async logout(): Promise<boolean> {
        try {
            const response = await fetch(`${API_URL}/account/logout`, {
                ...this.BASE_REQUEST_CONFIG,
                method: 'POST'
            });

            await goto('/login');
            window.location.reload();

            return response.ok;
        } catch {
            return false;
        }
    }

    static async getAccountInfo(): Promise<UserInfoResponse | null> {
        try {
            const response = await fetch(`${API_URL}/account/info`, {
                ...this.BASE_REQUEST_CONFIG,
                method: 'GET'
            });

            return response.ok ? await response.json() : null;
        } catch {
            return null;
        }
    }
}

async function createAuthStore() {
    const userInfo = await AuthService.getAccountInfo();
    const isLoggedIn = userInfo != null;
    const username = userInfo ? userInfo.username : null;

    const initialState: AuthState = {
        isLoggedIn: isLoggedIn,
        username: username
    };

    const { subscribe, set } = writable<AuthState>(initialState);

    return {
        subscribe,
        login: async (credentials: LoginRequest) => {
            const loginSuccess = await AuthService.login(credentials);

            if (!loginSuccess) {
                return false;
            }

            set({
                isLoggedIn: true,
                username: credentials.username
            });

            return true;
        },
        logout: async () => {
            const logoutSuccess = await AuthService.logout();

            if (!logoutSuccess) {
                return false;
            }

            set({
                isLoggedIn: false,
                username: null
            });

            return true;
        }
    };
}

export const authStore = await createAuthStore();
