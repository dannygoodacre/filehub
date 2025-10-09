import { useMutation } from '@tanstack/react-query';

const API_URL = import.meta.env.VITE_API_URL;

export default function useUpload() {
  return useMutation({
    mutationFn: async (data: { file: File; name: string; tags: string[] }): Promise<boolean> => {
      const formData = new FormData();

      formData.append('file', data.file);
      formData.append('name', data.name);

      data.tags.forEach((tag) => {
        formData.append('tags', tag);
      });

      const response = await fetch(`${API_URL}/files/upload`, {
        method: 'POST',
        credentials: 'include' as RequestCredentials,
        body: formData,
      });

      return response.ok;
    },
  });
}
