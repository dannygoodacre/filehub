import styles from './Login.module.scss';

import { NavBar, LoginBox } from '@/components';

export default function Login() {
  return (
    <div className={styles.container}>
      <NavBar />

      <div className={styles.login_box_container}>
        <LoginBox />
      </div>
    </div>
  );
}
