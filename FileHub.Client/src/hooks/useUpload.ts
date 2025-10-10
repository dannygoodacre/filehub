import { useMutation } from '@tanstack/react-query';

import { uploadFile } from '@/api/files';

export default function useUpload() {
  return useMutation({
    mutationFn: (data: { file: File; name: string; tags: string[] }) => uploadFile(data.file, data.name, data.tags),
  });
}
