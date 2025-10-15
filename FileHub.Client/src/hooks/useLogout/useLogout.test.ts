import React from 'react';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor, RenderHookResult } from '@testing-library/react';

import useLogout from './useLogout';

vi.mock('@/api', () => ({
  logout: vi.fn()
}));

import { logout } from '@/api';
import { UserInfo } from '@/types';

const queryClient = new QueryClient();
let wrapper: React.FC<{ children: React.ReactNode }>;
let result: RenderHookResult<ReturnType<typeof useLogout>, void>['result'];

beforeEach(() => {
  wrapper = ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);

  result = renderHook(() => useLogout(), { wrapper }).result;
});

describe('useLogout', () => {
  it('logs out and clears currentUser cache', async () => {
    // Arrange
    const userInfo: UserInfo = {
      username: 'test_user',
      isAccountConfirmed: true
    };

    queryClient.setQueryData(['currentUser'], userInfo);

    vi.mocked(logout).mockResolvedValue(undefined);

    const setQueryDataSpy = vi.spyOn(queryClient, 'setQueryData');

    // Act
    await result.current.mutateAsync();

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(logout).toHaveBeenCalledTimes(1);

    expect(setQueryDataSpy).toHaveBeenNthCalledWith(1, ['currentUser'], null);
  });

  it('handles logout failure', async () => {
    // Arrange
    const error = new Error('Test error');

    vi.mocked(logout).mockRejectedValueOnce(error);

    const userInfo: UserInfo = {
      username: 'test_user',
      isAccountConfirmed: true
    };

    queryClient.setQueryData(['currentUser'], userInfo);

    const setQueryDataSpy = vi.spyOn(queryClient, 'setQueryData');

    // Act
    try {
      await result.current.mutateAsync();
    } catch {}

    // Assert
    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toEqual(error);

    expect(setQueryDataSpy).not.toHaveBeenCalled();
  });
});
