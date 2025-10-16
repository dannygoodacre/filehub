import React from 'react';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor, RenderHookResult } from '@testing-library/react';

import useUpload from './useUpload';

vi.mock('@/api', () => ({
  uploadFile: vi.fn()
}));

import { uploadFile } from '@/api';

const queryClient = new QueryClient();
let wrapper: React.FC<{ children: React.ReactNode }>;
let result: RenderHookResult<ReturnType<typeof useUpload>, void>['result'];

beforeEach(() => {
  wrapper = ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);

  result = renderHook(() => useUpload(), { wrapper }).result;
});

describe('useUpload', () => {
  it('uploads a file', async () => {
    // Arrange
    vi.mocked(uploadFile).mockResolvedValue(true);

    const file = new File(['test file contents'], 'test-filename.txt', { type: 'text/plain' });

    const name = 'test name';

    const tags = ['tag 1', 'tag 2'];

    // Act
    await result.current.mutateAsync({ file, name, tags });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(uploadFile).toHaveBeenNthCalledWith(1, file, name, tags);
  });

  it('fails to upload a file', async () => {
    // Arrange
    const error = new Error('Test error');

    vi.mocked(uploadFile).mockRejectedValueOnce(error);

    const file = new File(['test file contents'], 'test-filename.txt', { type: 'text/plain' });

    const name = 'test name';

    const tags = ['tag 1', 'tag 2'];

    // Act
    try {
      await result.current.mutateAsync({ file, name, tags });
    } catch {}

    // Assert
    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toEqual(error);

    expect(uploadFile).toHaveBeenNthCalledWith(1, file, name, tags);
  });
});
