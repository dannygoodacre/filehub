import { useQuery } from '@tanstack/react-query';

import { getPaginatedFileMetadata } from '@/api/files/files';

export default function usePaginatedFileMetadata(page: number, size: number) {
  return useQuery({
    queryKey: ['fileMetadata', { page, size }],
    queryFn: () => getPaginatedFileMetadata(page, size)
  });
}
