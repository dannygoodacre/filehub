import React from 'react';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor } from '@testing-library/react';

import useAuth from './useAuth';

import { getCurrentUser } from '@/api';
import { UserInfo } from '@/types';

vi.mock('@/api', () => ({
  getCurrentUser: vi.fn()
}));

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false }
  }
});

let wrapper: React.FC<{ children: React.ReactNode }>;

beforeEach(() => {
  wrapper = ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);
});

describe('useAuth', () => {
  it('returns user data on success', async () => {
    // Arrange
    const userInfo: UserInfo = {
      username: 'test_user',
      isAccountConfirmed: true
    };

    vi.mocked(getCurrentUser).mockResolvedValueOnce(userInfo);

    // Act
    const { result } = renderHook(() => useAuth(), { wrapper });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(userInfo);

    expect(getCurrentUser).toHaveBeenCalledTimes(1);
  });

  it('handles error', async () => {
    // Arrange
    const error = new Error('Test error');

    vi.mocked(getCurrentUser).mockRejectedValueOnce(error);

    // Act
    const { result } = renderHook(() => useAuth(), { wrapper });

    // Assert
    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toEqual(error);

    expect(getCurrentUser).toHaveBeenCalledTimes(1);
  });
});
