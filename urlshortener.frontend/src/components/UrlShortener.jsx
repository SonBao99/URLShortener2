import React, { useState } from 'react';
import './UrlShortener.css';
import { useAuth } from '../AuthContext';
import QRModal from './QRModal';

const CopyIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
        <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
    </svg>
);

const QRIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
        <rect x="7" y="7" width="3" height="3"></rect>
        <rect x="14" y="7" width="3" height="3"></rect>
        <rect x="7" y="14" width="3" height="3"></rect>
        <rect x="14" y="14" width="3" height="3"></rect>
    </svg>
);

const ShareIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <circle cx="18" cy="5" r="3"></circle>
        <circle cx="6" cy="12" r="3"></circle>
        <circle cx="18" cy="19" r="3"></circle>
        <line x1="8.59" y1="13.51" x2="15.42" y2="17.49"></line>
        <line x1="15.41" y1="6.51" x2="8.59" y2="10.49"></line>
    </svg>
);

const OpenIcon = () => (
    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"></path>
        <polyline points="15 3 21 3 21 9"></polyline>
        <line x1="10" y1="14" x2="21" y2="3"></line>
    </svg>
);

const UrlShortener = ({ onUrlShortened }) => {
    const [originalUrl, setOriginalUrl] = useState('');
    const [customAlias, setCustomAlias] = useState('');
    const [shortenedUrl, setShortenedUrl] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [showShortenForm, setShowShortenForm] = useState(true);
    const [notification, setNotification] = useState('');
    const { user } = useAuth();
    const [showQR, setShowQR] = useState(false);
    const [showMyUrls, setShowMyUrls] = useState(false);

    const showNotification = (message) => {
        setNotification(message);
        setTimeout(() => setNotification(''), 3000); // Hide after 3 seconds
    };

    const handleShare = async () => {
        if (navigator.share) {
            try {
                await navigator.share({
                    title: 'Shared URL',
                    text: 'Check out this link:',
                    url: shortenedUrl
                });
            } catch (err) {
                // Fallback to clipboard if share is cancelled or fails
                await navigator.clipboard.writeText(shortenedUrl);
                showNotification('URL copied to clipboard!');
            }
        } else {
            // Fallback for browsers that don't support sharing
            await navigator.clipboard.writeText(shortenedUrl);
            showNotification('URL copied to clipboard!');
        }
    };

    const handleCopy = async () => {
        await navigator.clipboard.writeText(shortenedUrl);
        showNotification('URL copied to clipboard!');
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        setIsLoading(true);
        setError('');
        setShortenedUrl('');

        try {
            const headers = {
                'Content-Type': 'application/json'
            };
            
            if (user?.token) {
                headers['Authorization'] = `Bearer ${user.token}`;
            }

            const response = await fetch('https://localhost:7000/api/shorten', {
                method: 'POST',
                headers,
                body: JSON.stringify({
                    originalUrl: originalUrl,
                    customAlias: customAlias,
                }),
            });

            if (!response.ok) {
                throw new Error('Failed to shorten URL');
            }

            const data = await response.json();
            setShortenedUrl(data.shortenedUrl);

            // Save to localStorage if user is not logged in
            if (!user?.token) {
                const recentUrls = JSON.parse(localStorage.getItem('recentUrls') || '[]');
                const newUrl = {
                    originalUrl,
                    shortenedUrl: data.shortenedUrl,
                    createdAt: new Date().toISOString()
                };
                localStorage.setItem('recentUrls', JSON.stringify([newUrl, ...recentUrls].slice(0, 10)));
            }

            onUrlShortened({ originalUrl, shortenedUrl: data.shortenedUrl });
            setShowShortenForm(false);
        } catch (err) {
            setError('Failed to shorten the URL. Please try again.');
        } finally {
            setIsLoading(false);
        }
    };

    const handleShortenAnother = () => {
        setShowShortenForm(true);
        setShortenedUrl('');
        setOriginalUrl('');
        setCustomAlias('');
    };

    const handleMyUrlsClick = () => {
        if (user) {
            setShowMyUrls(true);
        } else {
            // Check if there are URLs in localStorage
            const recentUrls = JSON.parse(localStorage.getItem('recentUrls') || '[]');
            if (recentUrls.length > 0) {
                setShowMyUrls(true);
            } else {
                showNotification('No URLs in history. Create one first!');
            }
        }
    };

    return (
        <div className="url-shortener">
            {notification && (
                <div className="notification">
                    {notification}
                </div>
            )}
            {showShortenForm ? (
                <>
                    <div className="shortener-header">
                        <svg viewBox="0 0 24 24" fill="currentColor" className="link-icon">
                            <path d="M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z"/>
                        </svg>
                        <span>Shorten a long URL</span>
                    </div>
                    <form onSubmit={handleSubmit} className="shortener-form">
                        <input
                            type="url"
                            placeholder="Enter long link here"
                            value={originalUrl}
                            onChange={(e) => setOriginalUrl(e.target.value)}
                            className="input-field"
                            required
                        />
                        <div className="customize-section">
                            <div className="customize-header">
                                <svg viewBox="0 0 24 24" fill="currentColor" className="customize-icon">
                                    <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z"/>
                                </svg>
                                <span>Customize your link</span>
                            </div>
                            <div className="customize-inputs">
                                <select className="domain-select">
                                    <option value="linksnap.com">LinkSnap.com</option>
                                </select>
                                <input
                                    type="text"
                                    placeholder="Enter alias"
                                    value={customAlias}
                                    onChange={(e) => setCustomAlias(e.target.value)}
                                    className="alias-input"
                                />
                            </div>
                        </div>
                        <button type="submit" className="submit-button" disabled={isLoading}>
                            Shorten URL
                        </button>
                    </form>
                </>
            ) : (
                <>
                    <div className="result-container">
                        <div className="url-group">
                            <label>
                                <svg viewBox="0 0 24 24" fill="currentColor" className="link-icon">
                                    <path d="M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z"/>
                                </svg>
                                Your Long URL
                            </label>
                            <input type="text" value={originalUrl} readOnly className="result-input" />
                        </div>
                        <div className="url-group">
                            <label>LinkSnap</label>
                            <input type="text" value={shortenedUrl} readOnly className="result-input" />
                        </div>
                        <div className="action-buttons">
                            <button 
                                className="action-button back-button"
                                onClick={() => window.open(shortenedUrl, '_blank')}
                                title="Open link"
                            >
                                <OpenIcon /> Open
                            </button>
                            <button 
                                className="action-button qr-button"
                                onClick={() => setShowQR(true)}
                                title="Generate QR Code"
                            >
                                <QRIcon /> QR
                            </button>
                            <button 
                                className="action-button share-button"
                                onClick={handleShare}
                                title="Share URL"
                            >
                                <ShareIcon /> Share
                            </button>
                            <button 
                                className="action-button copy-button"
                                onClick={handleCopy}
                                title="Copy to clipboard"
                            >
                                <CopyIcon /> Copy
                            </button>
                        </div>
                        <div className="bottom-buttons">
                            <button 
                                className="secondary-button" 
                                onClick={handleMyUrlsClick}
                            >
                                My URLs
                            </button>
                            <button 
                                onClick={handleShortenAnother}
                                className="primary-button"
                            >
                                Shorten another
                            </button>
                        </div>
                    </div>
                </>
            )}
            {showQR && <QRModal url={shortenedUrl} onClose={() => setShowQR(false)} />}
        </div>
    );
};

export default UrlShortener;
