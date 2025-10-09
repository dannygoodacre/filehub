import { File, Film, Headphones } from "react-feather";

import styles from "./FileThumbnail.module.scss";

type FileThumbnailProps = {
  name: string;
  contentType: string;
  url: string;
};

export default function FileThumbnail({
  name,
  contentType,
  url,
}: FileThumbnailProps) {
  const generalType = contentType.split("/")[0];

  const renderFileIcon = () => {
    switch (generalType) {
      case "video":
        return <Film className={styles.file_icon} />;
      case "audio":
        return <Headphones className={styles.file_icon} />;
      default:
        return <File className={styles.file_icon} />;
    }
  };

  return (
    <a href={url} target="_blank" rel="noopener noreferrer">
      {generalType === "image" ? (
        <img src={url} alt={name} />
      ) : (
        <div className={styles.icon_outer_container}>
          <div className={styles.icon_inner_container}>{renderFileIcon()}</div>
        </div>
      )}
    </a>
  );
}
