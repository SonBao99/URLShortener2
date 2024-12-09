import React from 'react';
import './QRModal.css';

const QRModal = ({ url, onClose }) => {
    const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=${encodeURIComponent(url)}`;

    return (
        <div className="qr-overlay" onClick={(e) => {
            if (e.target.className === 'qr-overlay') onClose();
        }}>
            <div className="qr-modal">
                <button className="qr-close" onClick={onClose}>×</button>
                <h3>QR Code</h3>
                <div className="qr-content">
                    <img src={qrUrl} alt="QR Code" />
                    <p className="qr-url">{url}</p>
                </div>
            </div>
        </div>
    );
};

export default QRModal;