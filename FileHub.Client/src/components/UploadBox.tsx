import React, { useState } from 'react';

import { Upload, File as FileIcon, X } from 'react-feather';

import styles from './UploadBox.module.scss';

import { useUpload } from '@/hooks';

export default function UploadBox() {
  const [file, setFile] = useState<File | null>(null);

  const [name, setName] = useState<string>('');

  const [tags, setTags] = useState<string[]>([]);

  const [tagInput, setTagInput] = useState<string>('');

  const upload = useUpload();

  const canUpload = file && name && tags && tags.length > 0;

  function handleUpload() {
    if (!file) {
      return;
    }

    upload.mutate(
      { file, name, tags },
      {
        onSuccess: () => {
          removeFile();
        },
      },
    );
  }

  function handleFileSelect(event: React.ChangeEvent<HTMLInputElement>) {
    const input = event.target as HTMLInputElement;
    const files = input.files as FileList;

    if (files && files.length > 0) {
      setFile(files[0]);
    }
  }

  function removeFile() {
    setFile(null);
    setName('');
    setTags([]);
    setTagInput('');
  }

  function addTag() {
    const tag = tagInput.trim();

    if (tag && !tags.includes(tag)) {
      setTags([...tags, tag]);
      setTagInput('');
    }
  }

  function removeTag(index: number) {
    setTags(tags.filter((_, i) => i !== index));
  }

  function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === 'Enter') {
      event.preventDefault();
      addTag();
    }
  }

  function fileSize(size: number): string {
    const units = ['B', 'KB', 'MB', 'GB'];

    let i = 0;
    while (size >= 1000 && i < units.length - 1) {
      size /= 1000;
      i++;
    }

    return `${i == 0 ? size : size.toFixed(2)} ${units[i]}`;
  }

  return (
    <div className={styles.container}>
      <div className={styles.upload_box_container}>
        <h1 className={styles.title}>Upload</h1>

        <div className={styles.upload_box}>
          <input type="file" onChange={(e) => handleFileSelect(e)} onClick={() => upload.reset()} />
          <div>
            <Upload className={styles.upload_icon} size={48} />
            <p>Drop a file here or click to browse</p>
          </div>
        </div>

        {file && (
          <>
            <div className={styles.selected_file_container}>
              <div className={styles.selected_file_card}>
                <FileIcon className={styles.file_icon} size={20} />
                <div>
                  <p className={styles.file_name}>{file.name}</p>
                  <p className={styles.file_size}>{fileSize(file.size)}</p>
                </div>
              </div>
              <button className={styles.remove_file} onClick={removeFile}>
                <X size={25} />
              </button>
            </div>

            <div className={styles.name_input_container}>
              <input value={name} onChange={(e) => setName(e.target.value)} type="text" placeholder="Name" />
            </div>

            <div>
              {tags.length > 0 && (
                <div className={styles.tags_box}>
                  {tags.map((tag, i) => (
                    <button key={i} onClick={() => removeTag(i)} title="Remove tag">
                      {tag}
                    </button>
                  ))}
                </div>
              )}

              <div className={styles.tag_input_box}>
                <input
                  type="text"
                  value={tagInput}
                  onChange={(e) => setTagInput(e.target.value)}
                  onKeyDown={handleKeyDown}
                  placeholder="Tags"
                />
                <button onClick={addTag}>Add</button>
              </div>
            </div>

            <button className={styles.upload_btn} onClick={handleUpload} disabled={!canUpload}>
              {upload.isPending ? 'Uploading' : 'Upload'}
            </button>
          </>
        )}
      </div>

      {upload.isSuccess && (
        <div className={styles.upload_alert} role="alert">
          File uploaded successfully!
        </div>
      )}
    </div>
  );
}
