import { get, post } from '../client/client';

import type { Credentials, UserInfo } from '@/types';

export const getCurrentUser = () => get<UserInfo>('/account/info');

export const login = (credentials: Credentials) => post<void, Credentials>('/account/login', credentials);

export const logout = () => post<void>('/account/logout');
