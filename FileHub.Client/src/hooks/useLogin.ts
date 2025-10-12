import { useMutation, useQueryClient } from '@tanstack/react-query';

import type { Credentials, UserInfo } from '@/types';

import { getCurrentUser, login } from '@/api/auth/auth';

export default function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (credentials: Credentials): Promise<UserInfo> => {
      await login(credentials);

      return await getCurrentUser();
    },
    onSuccess: (userInfo) => {
      queryClient.setQueryData(['currentUser'], userInfo);
    }
  });
}
