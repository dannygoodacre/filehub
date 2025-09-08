import { API_URL } from '$lib';
import type { FileMetaData } from '$lib/models/file';

class FileService {
    public async getAllFilesWithTag(tag: string): Promise<FileMetaData[] | null> {
        try {
            const response = await fetch(`${API_URL}/files/tag/${tag}`, {
                credentials: 'include' as RequestCredentials,
                method: 'GET'
            });

            return response.ok ? await response.json() : null;
        } catch {
            return null;
        }
    }
    
    public async getPageCount(pageSize: number) : Promise<number | null> {
        try {
            const response = await fetch(`${API_URL}/files/pagecount?pageSize=${pageSize}`, {
                credentials: 'include' as RequestCredentials,
                method: 'GET'
            });
            
            return response.ok ? await response.json() : null;
        } catch {
            return null;
        }
    }

    public async getPaginatedFiles(page: number, count: number) : Promise<FileMetaData[] | null> {
        try {
            const response = await fetch(`${API_URL}/files?page=${page}&count=${count}`, {
                credentials: 'include' as RequestCredentials,
                method: 'GET'
            });

            return response.ok ? await response.json() : null;
        } catch {
            return null;
        }
    }

    public async uploadFile(file: File, name: string, tags: string[]): Promise<boolean> {
        try {
            const formData = new FormData();

            formData.append('file', file);

            formData.append('name', name);

            tags.forEach((tag) => {
                formData.append('tags', tag);
            });

            const response = await fetch(`${API_URL}/files/upload`, {
                credentials: 'include' as RequestCredentials,
                method: 'POST',
                body: formData
            });

            return response.ok;
        } catch {
            return false;
        }
    }
}

export const fileService = new FileService();
