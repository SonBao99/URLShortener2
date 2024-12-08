import { useState } from 'react';
import { useAuth } from '../AuthContext';
import './Auth.css';

const AuthForm = ({ onClose, initialMode }) => {
    const [isLogin, setIsLogin] = useState(initialMode === 'login');
    const [isClosing, setIsClosing] = useState(false);
    const [formData, setFormData] = useState({
        username: '',
        email: '',
        password: '',
        fullName: ''
    });
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login, register } = useAuth();

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            if (isLogin) {
                await login(formData.email, formData.password);
                onClose();
            } else {
                await register(
                    formData.username,
                    formData.email,
                    formData.password,
                    formData.fullName
                );
                setIsLogin(true);
            }
        } catch (err) {
            setError(err.response?.data?.message || 'An error occurred');
        } finally {
            setLoading(false);
        }
    };

    const handleClose = () => {
        setIsClosing(true);
        setTimeout(onClose, 300);
    };

    return (
        <div className="auth-overlay" onClick={(e) => {
            if (e.target.className === 'auth-overlay') handleClose();
        }}>
            <div className={`auth-container ${isClosing ? 'slide-out' : ''}`}>
                <button className="auth-close" onClick={handleClose}>×</button>
                <div className="auth-box">
                    <div className="auth-logo">
                        <h1>LINKSNAP</h1>
                        <p>Welcome to LinkSnap</p>
                    </div>

                    <form onSubmit={handleSubmit}>
                        {!isLogin && (
                            <>
                                <div className="form-group">
                                    <input
                                        type="text"
                                        name="username"
                                        value={formData.username}
                                        onChange={handleChange}
                                        placeholder="Username"
                                        required
                                    />
                                </div>
                                <div className="form-group">
                                    <input
                                        type="text"
                                        name="fullName"
                                        value={formData.fullName}
                                        onChange={handleChange}
                                        placeholder="Full Name"
                                        required
                                    />
                                </div>
                            </>
                        )}
                        <div className="form-group">
                            <input
                                type="email"
                                name="email"
                                value={formData.email}
                                onChange={handleChange}
                                placeholder="Email"
                                required
                            />
                        </div>
                        <div className="form-group">
                            <input
                                type="password"
                                name="password"
                                value={formData.password}
                                onChange={handleChange}
                                placeholder="Password"
                                required
                            />
                        </div>

                        {isLogin && (
                            <div className="auth-options">
                                <label className="remember-me">
                                    <input type="checkbox" /> Remember me
                                </label>
                                <a href="#" className="forgot-password">Forgot Password?</a>
                            </div>
                        )}

                        {error && <div className="error-message">{error}</div>}

                        <button type="submit" className="auth-submit" disabled={loading}>
                            {loading ? 'Processing...' : (isLogin ? 'Sign In' : 'Sign Up')}
                        </button>

                        <div className="auth-switch">
                            {isLogin ? "Don't have an account? " : "Already have an account? "}
                            <button
                                type="button"
                                onClick={() => setIsLogin(!isLogin)}
                                className="switch-button"
                            >
                                {isLogin ? 'Sign Up' : 'Sign In'}
                            </button>
                        </div>

                        <div className="social-login">
                            <p>Or login via:</p>
                            <div className="social-buttons">
                                <button type="button" className="social-btn facebook">f</button>
                                <button type="button" className="social-btn google">G</button>
                                <button type="button" className="social-btn twitter">t</button>
                                <button type="button" className="social-btn microsoft">M</button>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default AuthForm;