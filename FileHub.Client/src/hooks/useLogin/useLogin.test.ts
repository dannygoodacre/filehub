import React from 'react';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, act, waitFor, RenderHookResult } from '@testing-library/react';

import useLogin from './useLogin';

import { login, getCurrentUser } from '@/api';
import { UserInfo } from '@/types';

vi.mock('@/api', () => ({
  login: vi.fn(),
  getCurrentUser: vi.fn()
}));

const queryClient = new QueryClient();
let wrapper: React.FC<{ children: React.ReactNode }>;
let result: RenderHookResult<ReturnType<typeof useLogin>, void>['result'];

beforeEach(() => {
  wrapper = ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);

  result = renderHook(() => useLogin(), { wrapper }).result;
});

describe('useLogin', () => {
  it('logs in and returns user info', async () => {
    // Arrange
    const userInfo: UserInfo = {
      username: 'test_user',
      isAccountConfirmed: true
    };

    vi.mocked(login).mockResolvedValueOnce(undefined);

    vi.mocked(getCurrentUser).mockResolvedValueOnce(userInfo);

    const credentials = {
      username: 'test_user',
      password: 'test_password'
    };

    const setQueryDataSpy = vi.spyOn(queryClient, 'setQueryData');

    // Act
    await result.current.mutateAsync(credentials);

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(userInfo);

    expect(login).toHaveBeenNthCalledWith(1, credentials);

    expect(getCurrentUser).toHaveBeenNthCalledWith(1);

    expect(setQueryDataSpy).toHaveBeenNthCalledWith(1, ['currentUser'], userInfo);
  });

  it('handles login failure', async () => {
    // Arrange
    const error = new Error('Invalid credentials');

    vi.mocked(login).mockRejectedValueOnce(error);

    const credentials = {
      username: 'test_user',
      password: 'test_password'
    };

    // Act
    await act(async () => {
      try {
        await result.current.mutateAsync(credentials);
      } catch {}
    });

    // Assert
    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toEqual(error);

    expect(login).toHaveBeenNthCalledWith(1, credentials);

    expect(getCurrentUser).not.toHaveBeenCalled();
  });
});
