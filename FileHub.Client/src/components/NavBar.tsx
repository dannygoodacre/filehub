import styles from './NavBar.module.scss';

import { Link, useLocation, useNavigate } from 'react-router-dom';

import { useLogout, useAuth } from '@/hooks';

export default function NavBar() {
    const logoutMutation = useLogout();

    const location = useLocation();

    const { data: user} = useAuth();

    const navigate = useNavigate();

    return (
        user ?
            <nav className={styles.navbar}>
                <Link to="/" className={styles.title}>FileHub</Link>

                <div className={styles.links}>
                    <Link className={styles.link} to="/">Home</Link>
                    <Link className={styles.link} to="/upload">Upload</Link>
                </div>

                <div className={styles.right}>
                        <span className={styles.username}>{user?.username}</span>
                    <button className="logout" onClick={() => logoutMutation.mutate()}>
                        Logout
                    </button>
                </div>
            </nav>
        : (
            <nav className={styles.navbar}>
                <Link to="/login" className={styles.title}>FileHub</Link>

                <div className={styles.right}>
                    { location.pathname !== "/login" && (
                        <button className="login" onClick={() => navigate("/login")}>
                            Login
                        </button>
                    )}
                </div>
            </nav>
        )
    );
}
