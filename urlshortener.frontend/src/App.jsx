import './App.css';
import UrlShortener from './components/UrlShortener';

function App() {
    return (
        <div className="App">
            <header className="app-header">
                <h1>LinkSnap</h1>
                <nav className="nav-links">
                    <a href="#features">Features</a>
                    <a href="#pricing">Plans</a>
                    <a href="#blog">Blog</a>
                    <a href="#signin">Sign In</a>
                </nav>
            </header>
            <main className="app-main">
                <UrlShortener />
                <div className="text-content">
                    <h2 className="main-title">The Original URL Shortener</h2>
                    <p className="main-subtitle">Create shorter URLs with LinkSnap.</p>
                    <p className="additional-info">
                        Want more out of your link shortener? Track link analytics, use 
                        branded domains for fully custom links, and manage your links 
                        with our paid plans.
                    </p>
                    <div className="action-buttons">
                        <a href="#plans" className="view-plans-btn">View Plans</a>
                        <a href="#signup" className="create-account-btn">Create Free Account</a>
                    </div>
                    <div className="features-section">
                        <h3 className="features-title">LinkSnap plans include:</h3>
                        <div className="features-list">
                            <div className="feature-item">
                                <svg viewBox="0 0 24 24" fill="currentColor">
                                    <path d="M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z"/>
                                </svg>
                                <span>Detailed Link Analytics</span>
                            </div>
                            <div className="feature-item">
                                <svg viewBox="0 0 24 24" fill="currentColor">
                                    <path d="M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z"/>
                                </svg>
                                <span>Bulk Short URLs</span>
                            </div>
                            <div className="feature-item">
                                <svg viewBox="0 0 24 24" fill="currentColor">
                                    <path d="M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z"/>
                                </svg>
                                <span>Fully Branded Domains</span>
                            </div>
                            <div className="feature-item">
                                <svg viewBox="0 0 24 24" fill="currentColor">
                                    <path d="M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z"/>
                                </svg>
                                <span>Link Management Features</span>
                            </div>
                        </div>
                    </div>
                </div>
            </main>
        </div>
    );
}

export default App;
