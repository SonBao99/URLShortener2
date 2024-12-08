import { useState } from 'react';
import { AuthProvider } from './AuthContext';
import { useAuth } from './AuthContext';
import AuthForm from './components/AuthForm';
import UrlShortener from './components/UrlShortener';
import MyUrls from './components/MyUrls';
import './App.css';

const AppContent = () => {
    const { user, logout } = useAuth();
    const [showAuth, setShowAuth] = useState(false);
    const [initialAuthMode, setInitialAuthMode] = useState('login');
    const [showMyUrls, setShowMyUrls] = useState(false);

    const handleAuthClick = (mode) => {
        setInitialAuthMode(mode);
        setShowAuth(true);
    };

    return (
        <div className="App">
            <header className="app-header">
                <h1>LinkSnap</h1>
                <nav className="nav-links">
                    <a href="#features">Features</a>
                    <a href="#pricing">Plans</a>
                    <button 
                        onClick={() => setShowMyUrls(true)}
                        className="nav-button"
                    >
                        My URLs
                    </button>
                    {user ? (
                        <button onClick={logout} className="auth-button">
                            Logout
                        </button>
                    ) : (
                        <div className="auth-buttons">
                            <button 
                                onClick={() => handleAuthClick('login')} 
                                className="auth-button"
                            >
                                Sign In
                            </button>
                            <button 
                                onClick={() => handleAuthClick('register')} 
                                className="auth-button auth-button-primary"
                            >
                                Sign Up
                            </button>
                        </div>
                    )}
                </nav>
            </header>
            <main className="app-main">
                <UrlShortener onUrlShortened={(url) => {
                    const recentUrls = JSON.parse(localStorage.getItem('recentUrls') || '[]');
                    localStorage.setItem('recentUrls', JSON.stringify([url, ...recentUrls].slice(0, 10)));
                }} />
                {showMyUrls && <MyUrls onClose={() => setShowMyUrls(false)} />}
                {showAuth && (
                    <div className="auth-overlay" onClick={(e) => {
                        if (e.target.className === 'auth-overlay') {
                            setShowAuth(false);
                        }
                    }}>
                        <AuthForm 
                            onClose={() => setShowAuth(false)} 
                            initialMode={initialAuthMode}
                        />
                    </div>
                )}
            </main>
        </div>
    );
};

const App = () => {
    return (
        <AuthProvider>
            <AppContent />
        </AuthProvider>
    );
};

export default App;
