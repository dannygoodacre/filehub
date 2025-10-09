import { useMutation, useQueryClient } from '@tanstack/react-query';

import type { UserInfo } from '@/types/UserInfo';

const API_URL = import.meta.env.VITE_API_URL;

export default function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (credentials: { username: string; password: string }): Promise<UserInfo> => {
      const loginResponse = await fetch(`${API_URL}/account/login`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(credentials),
      });

      if (!loginResponse.ok) {
        throw new Error('Login failed');
      }

      const userInfoResponse = await fetch(`${API_URL}/account/info`, {
        credentials: 'include',
      });

      if (!userInfoResponse.ok) {
        throw new Error('User info failed');
      }

      return await userInfoResponse.json();
    },
    onSuccess: (userInfo) => {
      queryClient.setQueryData(['currentUser'], userInfo);
    },
  });
}
