import { useQuery } from '@tanstack/react-query';

import { getPageCount } from '@/api';

export default function usePageCount(size: number) {
  return useQuery({
    queryKey: ['page', { size }],
    queryFn: () => getPageCount(size)
  });
}
