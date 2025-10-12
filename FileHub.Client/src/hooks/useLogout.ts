import { useMutation, useQueryClient } from '@tanstack/react-query';

import { logout } from '@/api/auth/auth';

export default function useLogout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logout,
    onSuccess: () => {
      queryClient.setQueryData(['currentUser'], null);
    }
  });
}
