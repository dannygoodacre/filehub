import { useQuery } from '@tanstack/react-query';

import { getCurrentUser } from '@/api';

export default function useAuth() {
  return useQuery({
    queryKey: ['currentUser'],
    queryFn: getCurrentUser
  });
}
