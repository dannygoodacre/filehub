import { useMutation, useQueryClient } from "@tanstack/react-query";

const API_URL = import.meta.env.VITE_API_URL;

export default function useLogout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () =>
      fetch(`${API_URL}/account/logout`, {
        method: "POST",
        credentials: "include",
      }),
    onSuccess: () => {
      queryClient.setQueryData(["currentUser"], null);
    },
  });
}
