import { useMutation, useQueryClient } from '@tanstack/react-query';

import { logout } from '@/api';

export default function useLogout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logout,
    onSuccess: () => {
      queryClient.setQueryData(['currentUser'], null);
    }
  });
}
