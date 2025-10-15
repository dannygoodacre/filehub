import { expect } from 'vitest';

import { get, post } from '../client/client';

vi.mock('../client/client', () => ({
  get: vi.fn(),
  post: vi.fn()
}));

import { getCurrentUser, login, logout } from '@/api';
import { Credentials, UserInfo } from '@/types';

describe('auth', () => {
  it('getCurrentUser', async () => {
    // Arrange
    const userInfo: UserInfo = {
      username: 'test_username',
      isAccountConfirmed: true
    };

    (get as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(userInfo);

    // Act
    const result = await getCurrentUser();

    // Assert
    expect(get).toHaveBeenNthCalledWith(1, '/account/info');

    expect(result).toEqual(userInfo);
  });

  it('login', async () => {
    // Arrange
    const credentials: Credentials = {
      username: 'test_username',
      password: 'test_password'
    };

    const response = { ok: true };

    (post as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(response);

    // Act
    const result = await login(credentials);

    // Assert
    expect(post).toHaveBeenNthCalledWith(1, '/account/login', credentials);

    expect(result).toEqual(response);
  });

  it('logout', async () => {
    // Arrange
    const response = { ok: true };

    (post as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(response);

    // Act
    const result = await logout();

    // Assert
    expect(post).toHaveBeenNthCalledWith(1, '/account/logout');

    expect(result).toEqual(response);
  });
});
