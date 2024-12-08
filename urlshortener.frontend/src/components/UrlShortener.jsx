import React, { useState } from 'react';
import './UrlShortener.css';
import { useAuth } from '../AuthContext';

const UrlShortener = ({ onUrlShortened }) => {
    const [originalUrl, setOriginalUrl] = useState('');
    const [customAlias, setCustomAlias] = useState('');
    const [shortenedUrl, setShortenedUrl] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [showShortenForm, setShowShortenForm] = useState(true);
    const [notification, setNotification] = useState('');
    const { user } = useAuth();

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
            
            // Add authorization header only if user is logged in
            if (user?.token) {
                headers['Authorization'] = `Bearer ${user.token}`;
            }

            const response = await fetch('https://localhost:7162/shorten', {
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
                                <svg viewBox="0 0 24 24" fill="currentColor" width="20" height="20">
                                    <path d="M14 18l-1.4-1.4 4.6-4.6H3v-2h14.2l-4.6-4.6L14 4l7 7z"/>
                                </svg>
                            </button>
                            <button 
                                className="action-button qr-button"
                                onClick={() => {
                                    const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=${encodeURIComponent(shortenedUrl)}`;
                                    window.open(qrUrl, '_blank');
                                }}
                                title="Generate QR Code"
                            >
                                QR
                            </button>
                            <button 
                                className="action-button share-button"
                                onClick={handleShare}
                                title="Share URL"
                            >
                                Share
                            </button>
                            <button 
                                className="action-button copy-button"
                                onClick={handleCopy}
                                title="Copy to clipboard"
                            >
                                Copy
                            </button>
                        </div>
                        <div className="bottom-buttons">
                            <button className="secondary-button">My URLs</button>
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
        </div>
    );
};

export default UrlShortener;
