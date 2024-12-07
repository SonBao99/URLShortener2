import React, { useState } from 'react';

const UrlShortener = () => {
    const [originalUrl, setOriginalUrl] = useState('');
    const [customAlias, setCustomAlias] = useState('');
    const [shortenedUrl, setShortenedUrl] = useState('');

    const handleSubmit = async (event) => {
        event.preventDefault();

        const response = await fetch('https://localhost:7162/shorten', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                originalUrl: originalUrl,
                customAlias: customAlias,
            }),
        });

        if (response.ok) {
            const data = await response.json();
            setShortenedUrl(data.shortenedUrl);
        } else {
            console.error('Failed to shorten the URL');
        }
    };

    return (
        <div>
            <h1>URL Shortener</h1>
            <form onSubmit={handleSubmit}>
                <div>
                    <label>Original URL:</label>
                    <input
                        type="text"
                        value={originalUrl}
                        onChange={(e) => setOriginalUrl(e.target.value)}
                        required
                    />
                </div>
                <div>
                    <label>Custom Alias (Optional):</label>
                    <input
                        type="text"
                        value={customAlias}
                        onChange={(e) => setCustomAlias(e.target.value)}
                    />
                </div>
                <button type="submit">Shorten URL</button>
            </form>
            {shortenedUrl && (
                <div>
                    <p>Shortened URL: <a href={shortenedUrl} target="_blank" rel="noopener noreferrer">{shortenedUrl}</a></p>
                </div>
            )}
        </div>
    );
};

export default UrlShortener;
