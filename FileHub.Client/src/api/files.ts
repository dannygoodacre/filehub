import { get, post } from './client';

import { FileMetadata } from '@/types';

export const getPageCount = (size: number) => get<number>(`/files/pagecount?pageSize=${size}`);

export const getPaginatedFileMetadata = (page: number, size: number) => get<FileMetadata[]>(`/files/${page}/${size}`);

export const uploadFile = (file: File, name: string, tags: string[]) => {
  const formData = new FormData();

  formData.append('file', file);
  formData.append('name', name);

  tags.forEach((tag) => formData.append('tags', tag));

  return post<Boolean, File>('files/upload', file);
};
