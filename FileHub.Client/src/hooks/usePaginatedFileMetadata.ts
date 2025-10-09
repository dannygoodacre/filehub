import { useQuery } from '@tanstack/react-query';

import { FileMetadata } from '@/types';

const API_URL = import.meta.env.VITE_API_URL;

async function fetchFileMetadata(page: number, size: number): Promise<FileMetadata[]> {
  const result = await fetch(`${API_URL}/files?page=${page}&count=${size}`, {
    credentials: 'include' as RequestCredentials,
  });

  if (!result.ok) {
    throw new Error();
  }

  return result.json();
}

export function usePaginatedFileMetadata(page: number, size: number) {
  return useQuery({
    queryKey: ['fileMetadata', { page, size }],
    queryFn: () => fetchFileMetadata(page, size),
  });
}
