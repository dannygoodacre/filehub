export interface FileMetadata {
  id: string;
  name: string;
  accessLocation: string;
  contentType: string;
  createdAt: Date;
  uploader: string;
  tags: string[];
}
