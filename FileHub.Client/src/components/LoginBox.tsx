import { type FormEvent, useState } from 'react';

import styles from './LoginBox.module.scss';

import { useLogin, useAuth } from '@/hooks';

export default function LoginBox() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const [showUsernameError, setShowUsernameError] = useState(false);
  const [showPasswordError, setShowPasswordError] = useState(false);

  const login = useLogin();
  const { data: user } = useAuth();

  const clearInputs = () => {
    setUsername('');
    setPassword('');
  };

  const clearErrors = () => {
    setShowUsernameError(false);
    setShowPasswordError(false);

    login.reset();
  };

  const handleLogin = (event: FormEvent) => {
    event.preventDefault();

    let hasError = false;

    if (!username) {
      setShowUsernameError(true);

      hasError = true;
    }

    if (!password) {
      setShowPasswordError(true);

      hasError = true;
    }

    if (hasError) {
      return;
    }

    login.mutate(
      { username, password },
      {
        onSuccess: clearInputs,
        onError: clearInputs,
      },
    );
  };

  return (
    <div className={styles.container}>
      <form className={styles.form} onSubmit={handleLogin}>
        <h1 className={styles.title}>Login</h1>

        <div className={`${styles.form_field} ${showUsernameError ? styles.input_box_error : ''}`}>
          <label>Username</label>
          <input
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            onClick={() => login.reset()}
            onInput={() => clearErrors()}
            type="text"
            placeholder="Username"
          />
          {showUsernameError && <p>Please enter a username</p>}
        </div>

        <div className={`${styles.form_field} ${showPasswordError ? styles.input_box_error : ''}`}>
          <label>Password</label>
          <input
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            onClick={() => login.reset()}
            onInput={() => clearErrors()}
            type="password"
            placeholder="**********"
          />
          {showPasswordError && <p>Please enter a password</p>}
        </div>

        <button className={styles.button} type="submit">
          {login.isPending ? 'Logging in...' : 'Login'}
        </button>
      </form>

      {login.isSuccess && user && (
        <div className={styles.alert_success} role="alert">
          <span>Login successful</span>
        </div>
      )}

      {login.isError && (
        <div className={styles.alert_failure} role="alert">
          <span>Login failed</span>
        </div>
      )}
    </div>
  );
}
