import { expect } from 'vitest';

import { get, post } from '../client/client';

import { getCurrentUser, login, logout } from '@/api';
import { Credentials, UserInfo } from '@/types';

vi.mock('../client/client', () => ({
  get: vi.fn(),
  post: vi.fn()
}));

describe('auth', () => {
  it('getCurrentUser', async () => {
    // Arrange
    const mockUser: UserInfo = {
      username: 'test_username',
      isAccountConfirmed: true
    };

    (get as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(mockUser);

    // Act
    const result = await getCurrentUser();

    // Assert
    expect(get).toHaveBeenCalledWith('/account/info');

    expect(result).toEqual(mockUser);
  });

  it('login', async () => {
    // Arrange
    const credentials: Credentials = {
      username: 'test_username',
      password: 'test_password'
    };

    const mockResponse = { ok: true };

    (post as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(mockResponse);

    // Act
    const result = await login(credentials);

    // Assert
    expect(post).toHaveBeenCalledWith('/account/login', credentials);

    expect(result).toEqual(mockResponse);
  });

  it('logout', async () => {
    // Arrange
    const mockResponse = { ok: true };

    (post as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(mockResponse);

    // Act
    const result = await logout();

    // Assert
    expect(post).toHaveBeenCalledWith('/account/logout');

    expect(result).toEqual(mockResponse);
  });
});
