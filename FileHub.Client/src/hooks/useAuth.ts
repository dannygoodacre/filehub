import { useQuery } from '@tanstack/react-query';

import { getCurrentUser } from '@/api/auth/auth';

export default function useAuth() {
  return useQuery({
    queryKey: ['currentUser'],
    queryFn: getCurrentUser
  });
}
