import React from 'react';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';

import usePaginatedFileMetadata from './usePaginatedFileMetadata';

vi.mock('@/api', () => ({
  getPaginatedFileMetadata: vi.fn()
}));

import { getPaginatedFileMetadata } from '@/api';
import { FileMetadata } from '@/types';

const queryClient = new QueryClient();
let wrapper: React.FC<{ children: React.ReactNode }>;

beforeEach(() => {
  wrapper = ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);
});

describe('usePaginatedFileMetadata', () => {
  it('gets paginated file metadata', async () => {
    // Arrange
    const fileMetadata: FileMetadata[] = [
      {
        id: '123',
        name: 'test name 1',
        accessLocation: 'test access location 1',
        contentType: 'test/content-type-1',
        createdAt: '2025-09-01',
        uploader: 'test_user1',
        tags: ['tag 1', 'tag 2']
      },
      {
        id: '456',
        name: 'test name 2',
        accessLocation: 'test access location 2',
        contentType: 'test/content-type-2',
        createdAt: '2025-09-01',
        uploader: 'test_user2',
        tags: ['tag 3', 'tag 4']
      }
    ];

    vi.mocked(getPaginatedFileMetadata).mockResolvedValueOnce(fileMetadata);

    const page = 3;
    const size = 5;

    // Act
    const { result } = renderHook(() => usePaginatedFileMetadata(page, size), { wrapper });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(fileMetadata);

    expect(getPaginatedFileMetadata).toHaveBeenNthCalledWith(1, page, size);
  });
});
