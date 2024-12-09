import React, { useState, useEffect } from 'react';
import './MyUrls.css';
import { useAuth } from '../AuthContext';

const MyUrls = ({ onClose }) => {
    const [recentUrls, setRecentUrls] = useState([]);
    const [isClosing, setIsClosing] = useState(false);
    const [showClearWarning, setShowClearWarning] = useState(false);
    const { user } = useAuth();

    useEffect(() => {
        const fetchUrls = async () => {
            if (user?.token) {
                try {
                    const response = await fetch('https://localhost:7000/api/urls', {
                        headers: {
                            'Authorization': `Bearer ${user.token}`
                        }
                    });
                    if (response.ok) {
                        const data = await response.json();
                        setRecentUrls(data);
                    }
                } catch (error) {
                    console.error('Failed to fetch URLs:', error);
                }
            } else {
                // If user is not logged in, use localStorage
                const urls = JSON.parse(localStorage.getItem('recentUrls') || '[]');
                setRecentUrls(urls);
            }
        };

        fetchUrls();
    }, [user]);

    const handleClose = () => {
        setIsClosing(true);
        setTimeout(onClose, 300);
    };

    const handleClearHistory = async () => {
        if (user?.token) {
            // Add API endpoint to clear user's URLs
            try {
                const response = await fetch('https://localhost:7000/api/urls/clear', {
                    method: 'DELETE',
                    headers: {
                        'Authorization': `Bearer ${user.token}`
                    }
                });
                if (response.ok) {
                    setRecentUrls([]);
                }
            } catch (error) {
                console.error('Failed to clear URLs:', error);
            }
        } else {
            localStorage.setItem('recentUrls', '[]');
            setRecentUrls([]);
        }
        setShowClearWarning(false);
    };

    const getFaviconUrl = (url) => {
        try {
            const domain = new URL(url).hostname;
            return `https://www.google.com/s2/favicons?domain=${domain}&sz=32`;
        } catch {
            return '/favicon.ico';
        }
    };

    return (
        <div className="my-urls-overlay" onClick={(e) => {
            if (e.target.className === 'my-urls-overlay') handleClose();
        }}>
            <div className={`my-urls-container ${isClosing ? 'slide-out' : ''}`}>
                <div className="my-urls-header">
                    <h2>Your recent TinyURLs</h2>
                    <button className="close-button" onClick={handleClose}>×</button>
                </div>
                <div className="urls-list">
                    {recentUrls.length === 0 ? (
                        <div className="no-urls">
                            No more recent URLs in your history
                        </div>
                    ) : (
                        <>
                            {recentUrls.map((url, index) => (
                                <div key={index} className="url-item">
                                    <div className="url-info">
                                        <div className="short-url-row">
                                            <img 
                                                src={getFaviconUrl(url.originalUrl)} 
                                                alt="favicon" 
                                                className="url-favicon" 
                                                onError={(e) => e.target.src = '/favicon.ico'}
                                            />
                                            <a 
                                                href={url.shortenedUrl} 
                                                target="_blank" 
                                                rel="noopener noreferrer" 
                                                className="short-url"
                                            >
                                                {url.shortenedUrl}
                                            </a>
                                            <span className="affiliate-tag">
                                                <span>Affiliate link</span>
                                                <span className="info-icon">ℹ</span>
                                            </span>
                                        </div>
                                        <div className="original-url">
                                            <a 
                                                href={url.originalUrl}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="original-url"
                                            >
                                                {url.originalUrl}
                                            </a>
                                        </div>
                                        <div className="url-meta">
                                            <span>Tracking Disabled</span>
                                            <span>•</span>
                                            <span>an hour ago</span>
                                        </div>
                                        <div className="url-actions">
                                            <button 
                                                className="action-btn primary"
                                                onClick={() => navigator.clipboard.writeText(url.shortenedUrl)}
                                                title="Copy to clipboard"
                                            >
                                                Copy
                                            </button>
                                            <button 
                                                className="action-btn secondary"
                                                onClick={() => {
                                                    const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=${encodeURIComponent(url.shortenedUrl)}`;
                                                    window.open(qrUrl, '_blank');
                                                }}
                                            >
                                                QR
                                            </button>
                                            <button 
                                                className="action-btn secondary"
                                                onClick={() => {
                                                    if (navigator.share) {
                                                        navigator.share({
                                                            title: 'Shared URL',
                                                            text: 'Check out this link:',
                                                            url: url.shortenedUrl
                                                        });
                                                    } else {
                                                        navigator.clipboard.writeText(url.shortenedUrl);
                                                        alert('Link copied to clipboard!');
                                                    }
                                                }}
                                            >
                                                Share
                                            </button>
                                            <button 
                                                className="action-btn secondary"
                                                onClick={() => window.open(url.shortenedUrl, '_blank')}
                                            >
                                                Open
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            ))}
                            <div className="clear-history-container">
                                <button className="clear-history" onClick={handleClearHistory}>
                                    Clear History
                                </button>
                            </div>
                        </>
                    )}
                </div>
                {showClearWarning && (
                    <div className="clear-warning-overlay">
                        <div className="clear-warning-modal">
                            <h3>Clear History?</h3>
                            <p>Are you sure you want to clear all your URL history? This action cannot be undone.</p>
                            <div className="clear-warning-actions">
                                <button className="cancel-clear" onClick={() => setShowClearWarning(false)}>
                                    Cancel
                                </button>
                                <button className="confirm-clear" onClick={handleClearHistory}>
                                    Clear History
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default MyUrls;