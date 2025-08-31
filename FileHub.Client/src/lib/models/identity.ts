export interface LoginRequest {
    username: string;
    password: string;
}

export interface UserInfoResponse {
    id: number;
    username: string;
    isAccountConfirmed: boolean;
}

export interface AuthState {
    isLoggedIn: boolean;
    username: string | null;
}
