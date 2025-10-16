import styles from './Upload.module.scss';

import { NavBar, UploadBox } from '@/components';

export default function Upload() {
  return (
    <div className={styles.container}>
      <NavBar />

      <div className={styles.upload_box_container}>
        <UploadBox />
      </div>
    </div>
  );
}
