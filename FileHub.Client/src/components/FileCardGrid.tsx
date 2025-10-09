import styles from "./FileCardGrid.module.scss";

import { FileCard } from "@/components/index";
import { FileMetadata } from "@/types";

type FileCardGridProps = {
  fileMetadata: FileMetadata[];
};

export default function FileCardGrid({ fileMetadata }: FileCardGridProps) {
  return (
    <div className={styles.container}>
      {fileMetadata.map((metadata, index) => (
        <FileCard key={index} fileMetadata={metadata} />
      ))}
    </div>
  );
}
