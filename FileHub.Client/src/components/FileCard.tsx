import styles from "./FileCard.module.scss";

import { FileMetadata } from "@/types";
import { FileThumbnail } from "@/components/index";

type FileCardProps = {
  fileMetadata: FileMetadata;
};

export default function FileCard({ fileMetadata }: FileCardProps) {
  return (
    <div className={styles.container}>
      <div className={styles.thumbnail_container}>
        <FileThumbnail
          name={fileMetadata.name}
          url={fileMetadata.accessLocation}
          contentType={fileMetadata.contentType}
        />
      </div>

      <div className={styles.name_container}>{fileMetadata.name}</div>

      <div className={styles.content_type_container}>
        {fileMetadata.contentType}
      </div>

      <div className={styles.tags_container}>
        {fileMetadata.tags.map((tag, index) => (
          <span key={index}>{tag}</span>
        ))}
      </div>
    </div>
  );
}
