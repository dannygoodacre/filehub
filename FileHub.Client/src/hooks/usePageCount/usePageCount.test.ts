import React from 'react';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';

import usePageCount from './usePageCount';

vi.mock('@/api', () => ({
  getPageCount: vi.fn()
}));

import { getPageCount } from '@/api';

const queryClient = new QueryClient();
let wrapper: React.FC<{ children: React.ReactNode }>;

beforeEach(() => {
  wrapper = ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);
});

describe('usePageCount', () => {
  it('gets page count', async () => {
    // Arrange
    const pageCount = 3;

    vi.mocked(getPageCount).mockResolvedValueOnce(pageCount);

    const pageSize = 2;

    // Act
    const { result } = renderHook(() => usePageCount(pageSize), { wrapper });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(pageCount);

    expect(getPageCount).toHaveBeenNthCalledWith(1, pageSize);
  });
});
