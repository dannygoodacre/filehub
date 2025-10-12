import { describe, expect } from 'vitest';

import { get, post } from '../client/client';

import { getPageCount, getPaginatedFileMetadata, uploadFile } from '@/api';
import { FileMetadata } from '@/types';

vi.mock('../client/client', () => ({
  get: vi.fn(),
  post: vi.fn()
}));

describe('files', () => {
  it('getPageCount', async () => {
    // Arrange
    const size = 5;

    const mockNumber = 3;

    (get as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(mockNumber);

    // Act
    const result = await getPageCount(size);

    // Assert
    expect(get).toHaveBeenCalledWith(`/files/pagecount?pageSize=${size}`);

    expect(result).toEqual(mockNumber);
  });

  it('getPaginatedFileMetadata', async () => {
    // Arrange
    const page = 3;

    const size = 2;

    const mockFileMetadata: FileMetadata[] = [
      {
        id: '123',
        name: 'test name 1',
        accessLocation: 'url 1',
        contentType: 'test/content-type-1',
        createdAt: '2025-10-09',
        uploader: 'test_user',
        tags: ['tag 1', 'tag 2']
      },
      {
        id: '456',
        name: 'test name 2',
        accessLocation: 'url 2',
        contentType: 'test/content-type-2',
        createdAt: '2025-10-09',
        uploader: 'test_user',
        tags: ['tag 3', 'tag 4']
      }
    ];

    (get as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(mockFileMetadata);

    // Act
    const result = await getPaginatedFileMetadata(page, size);

    // Assert
    expect(get).toHaveBeenCalledWith(`/files?page=${page}&count=${size}`);

    expect(result).toEqual(mockFileMetadata);
  });

  it('uploadFile', async () => {
    // Arrange
    const file = new File(['test file contents'], 'test-filename.txt', { type: 'text/plain' });

    const name = 'test name';

    const tags = ['tag 1', 'tag 2'];

    const formData = new FormData();

    formData.append('file', name);
    formData.append('name', name);
    tags.forEach((tag) => formData.append('tags', tag));

    const mockResponse = true;

    (post as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce(mockResponse);

    // Act
    const result = await uploadFile(file, name, tags);

    // Assert
    expect(post).toHaveBeenCalledWith('/files/upload', expect.any(FormData));

    const actualFormData = (post as any).mock.calls[0][1] as FormData;

    expect(actualFormData.get('file')).toBe(file);
    expect(actualFormData.get('name')).toBe(name);
    expect(actualFormData.getAll('tags')).toEqual(tags);

    expect(result).toEqual(mockResponse);
  });
});
