import React, { useState, useEffect } from 'react';
import './MyUrls.css';
import { useAuth } from '../AuthContext';
import QRModal from './QRModal';

// Add these SVG icons as components
const CopyIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
        <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
    </svg>
);

const QRIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
        <rect x="7" y="7" width="3" height="3"></rect>
        <rect x="14" y="7" width="3" height="3"></rect>
        <rect x="7" y="14" width="3" height="3"></rect>
        <rect x="14" y="14" width="3" height="3"></rect>
    </svg>
);

const ShareIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <circle cx="18" cy="5" r="3"></circle>
        <circle cx="6" cy="12" r="3"></circle>
        <circle cx="18" cy="19" r="3"></circle>
        <line x1="8.59" y1="13.51" x2="15.42" y2="17.49"></line>
        <line x1="15.41" y1="6.51" x2="8.59" y2="10.49"></line>
    </svg>
);

const OpenIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"></path>
        <polyline points="15 3 21 3 21 9"></polyline>
        <line x1="10" y1="14" x2="21" y2="3"></line>
    </svg>
);

const MyUrls = ({ onClose }) => {
    const [recentUrls, setRecentUrls] = useState([]);
    const [isClosing, setIsClosing] = useState(false);
    const [showClearWarning, setShowClearWarning] = useState(false);
    const [showQR, setShowQR] = useState(false);
    const [selectedUrl, setSelectedUrl] = useState('');
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

    const formatTimeAgo = (dateString) => {
        const date = new Date(dateString);
        const now = new Date();
        const seconds = Math.floor((now - date) / 1000);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
        const days = Math.floor(hours / 24);

        if (days > 0) return `${days} day${days > 1 ? 's' : ''} ago`;
        if (hours > 0) return `${hours} hour${hours > 1 ? 's' : ''} ago`;
        if (minutes > 0) return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
        return 'just now';
    };

    const handleQRClick = (url) => {
        setSelectedUrl(url);
        setShowQR(true);
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
                                            <span>{url.usageCount} click{url.usageCount !== 1 ? 's' : ''}</span>
                                            <span>|</span>
                                            <span>{formatTimeAgo(url.createdAt)}</span>
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
                                                onClick={() => handleQRClick(url.shortenedUrl)}
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
                {showQR && <QRModal url={selectedUrl} onClose={() => setShowQR(false)} />}
            </div>
        </div>
    );
};

export default MyUrls;