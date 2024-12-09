import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './AuthContext';
import AuthCallback from './components/AuthCallback';
import { useState } from 'react';
import { useAuth } from './AuthContext';
import UrlShortener from './components/UrlShortener';
import MyUrls from './components/MyUrls';
import AuthForm from './components/AuthForm';
import Profile from './components/Profile';
import { UserProfileButton } from './components/Profile';
import './App.css';

const LinkIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"></path>
    <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"></path>
  </svg>
);

const AppContent = () => {
    const { user, logout } = useAuth();
    const [showAuth, setShowAuth] = useState(false);
    const [initialAuthMode, setInitialAuthMode] = useState('login');
    const [showMyUrls, setShowMyUrls] = useState(false);
    const [showProfile, setShowProfile] = useState(false);

    const handleAuthClick = (mode) => {
        setInitialAuthMode(mode);
        setShowAuth(true);
    };

    return (
        <div className="App">
            <header className="app-header">
                <h1>LinkSnap</h1>
                <nav className="nav-links">
                    
                    <button
                        onClick={() => setShowMyUrls(true)}
                        className="nav-button"
                    >
                        <LinkIcon />
                        My URLs
                    </button>
                    {user ? (
                        <div className="auth-buttons">
                            <UserProfileButton
                                user={user}
                                onClick={() => setShowProfile(true)}
                            />
                            <button onClick={logout} className="auth-button">
                                Logout
                            </button>
                        </div>
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
                {showProfile && <Profile onClose={() => setShowProfile(false)} />}
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
        <Router>
            <AuthProvider>
                <Routes>
                    <Route path="/auth-callback" element={<AuthCallback />} />
                    <Route path="/*" element={<AppContent />} />
                </Routes>
            </AuthProvider>
        </Router>
    );
};

export default App;
