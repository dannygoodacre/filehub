import { useState } from 'react';

import { ChevronLeft, ChevronRight } from 'react-feather';

import styles from './Home.module.scss';

import { NavBar, FileCardGrid } from '@/components';
import { usePageCount } from '@/hooks/usePageCount';
import { usePaginatedFileMetadata } from '@/hooks/usePaginatedFileMetadata';

export default function Home() {
  const pageSize = 15;
  const maxVisiblePageButtons = 5;

  const [page, setPage] = useState<number>(1);

  const pageCount = usePageCount(pageSize);
  const fileMetadata = usePaginatedFileMetadata(page, pageSize);

  let buttonLabels: number[] = Array.from({ length: pageCount.data! }, (_, i) => i + 1);

  let start = Math.max(
    0,
    Math.min(pageCount.data! - maxVisiblePageButtons, page - Math.floor(maxVisiblePageButtons / 2) - 1),
  );

  let end = Math.min(pageCount.data!, start + maxVisiblePageButtons);

  let visibleButtonLabels: number[] = buttonLabels.slice(start, end);

  return (
    <>
      <NavBar />

      {fileMetadata.data && (
        <>
          <div className={styles.grid_container}>
            <FileCardGrid fileMetadata={fileMetadata.data} />
          </div>

          <div className={styles.nav_button_container}>
            <button className={styles.nav_button} onClick={() => setPage(page - 1)} disabled={page === 1}>
              <ChevronLeft />
            </button>

            <div className={styles.page_buttons_container}>
              {visibleButtonLabels.map((label, index) => (
                <button
                  key={index}
                  className={`${styles.page_button} ${label == page ? styles.current_page_button : ''}`}
                  onClick={() => setPage(label)}>
                  {label}
                </button>
              ))}
            </div>

            <button className={styles.nav_button} onClick={() => setPage(page + 1)} disabled={page === pageCount.data}>
              <ChevronRight />
            </button>
          </div>
        </>
      )}
    </>
  );
}
