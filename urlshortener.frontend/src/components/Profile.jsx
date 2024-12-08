import React, { useState, useEffect } from 'react';
import './Profile.css';
import { useAuth } from '../AuthContext';

export const UserProfileButton = ({ user, onClick }) => {
    return (
        <button 
            className="profile-button" 
            onClick={onClick}
        >
            {user.avatar ? (
                <img 
                    src={user.avatar} 
                    alt="Profile" 
                    className="profile-button-avatar" 
                />
            ) : (
                <div className="profile-button-placeholder">
                    {user?.fullName?.charAt(0) || 'U'}
                </div>
            )}
            <span className="profile-button-name">
                {user?.fullName || 'User'}
            </span>
        </button>
    );
};

const Profile = ({ onClose }) => {
    const { user, updateUser } = useAuth();
    const [isClosing, setIsClosing] = useState(false);
    const [activeTab, setActiveTab] = useState('general');
    const [formData, setFormData] = useState({
        fullName: '',
        currentPassword: '',
        newPassword: '',
        confirmPassword: '',
    });
    const [avatar, setAvatar] = useState(null);
    const [notification, setNotification] = useState('');

    // Load user data when component mounts
    useEffect(() => {
        if (user) {
            setFormData(prev => ({
                ...prev,
                fullName: user.fullName || ''
            }));
            setAvatar(user.avatar || null);
        }
    }, [user]);

    const handleClose = () => {
        setIsClosing(true);
        setTimeout(onClose, 300);
    };

    const showNotification = (message) => {
        setNotification(message);
        setTimeout(() => setNotification(''), 3000);
    };

    const handleAvatarChange = async (event) => {
        const file = event.target.files[0];
        if (file) {
            try {
                const formData = new FormData();
                formData.append('avatar', file);

                const response = await fetch('https://localhost:7214/api/auth/update-avatar', {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${user.token}`
                    },
                    body: formData
                });

                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || 'Failed to update avatar');
                }

                const reader = new FileReader();
                reader.onloadend = () => {
                    setAvatar(reader.result);
                    updateUser({ ...user, avatar: reader.result });
                    showNotification('Avatar updated successfully!');
                };
                reader.readAsDataURL(file);
            } catch (error) {
                showNotification(error.message || 'Failed to update avatar');
            }
        }
    };

    const handlePasswordChange = async (e) => {
        e.preventDefault();
        if (formData.newPassword !== formData.confirmPassword) {
            showNotification('New passwords do not match!');
            return;
        }

        try {
            const response = await fetch('https://localhost:7214/api/auth/change-password', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${user.token}`
                },
                body: JSON.stringify({
                    currentPassword: formData.currentPassword,
                    newPassword: formData.newPassword
                })
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to update password');
            }

            showNotification('Password updated successfully!');
            setFormData(prev => ({
                ...prev,
                currentPassword: '',
                newPassword: '',
                confirmPassword: ''
            }));
        } catch (error) {
            showNotification(error.message || 'Failed to update password');
        }
    };

    const handleGeneralInfoUpdate = async (e) => {
        e.preventDefault();
        try {
            const response = await fetch('https://localhost:7214/api/auth/update-profile', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${user.token}`
                },
                body: JSON.stringify({
                    fullName: formData.fullName
                })
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to update profile');
            }

            const updatedUser = { ...user, fullName: formData.fullName };
            updateUser(updatedUser);
            showNotification('Profile updated successfully!');
        } catch (error) {
            showNotification(error.message || 'Failed to update profile');
        }
    };

    return (
        <div className="profile-overlay" onClick={(e) => {
            if (e.target.className === 'profile-overlay') handleClose();
        }}>
            <div className={`profile-container ${isClosing ? 'slide-out' : ''}`}>
                <button className="profile-close" onClick={handleClose}>×</button>
                
                {notification && (
                    <div className="profile-notification">
                        {notification}
                    </div>
                )}

                <div className="profile-content">
                    <div className="profile-header">
                        <div className="avatar-wrapper">
                            {avatar ? (
                                <img src={avatar} alt="Profile" className="avatar" />
                            ) : (
                                <div className="avatar-placeholder">
                                    {user?.fullName?.charAt(0) || 'U'}
                                </div>
                            )}
                            <label className="avatar-change">
                                <input
                                    type="file"
                                    accept="image/*"
                                    onChange={handleAvatarChange}
                                    hidden
                                />
                                Change Avatar
                            </label>
                        </div>
                        <h2 className="profile-name">{user?.fullName || 'User'}</h2>
                        <p className="profile-email">{user?.email}</p>
                    </div>

                    <nav className="profile-nav">
                        <button 
                            className={`nav-link ${activeTab === 'general' ? 'active' : ''}`}
                            onClick={() => setActiveTab('general')}
                        >
                            General
                        </button>
                        <button 
                            className={`nav-link ${activeTab === 'security' ? 'active' : ''}`}
                            onClick={() => setActiveTab('security')}
                        >
                            Security
                        </button>
                    </nav>

                    <div className="tab-content">
                        {activeTab === 'general' ? (
                            <form onSubmit={handleGeneralInfoUpdate} className="profile-form">
                                <div className="form-field">
                                    <label>Full Name</label>
                                    <input
                                        type="text"
                                        value={formData.fullName}
                                        onChange={(e) => setFormData({...formData, fullName: e.target.value})}
                                        placeholder="Enter your full name"
                                    />
                                </div>
                                <button type="submit" className="save-button">
                                    Save Changes
                                </button>
                            </form>
                        ) : (
                            <form onSubmit={handlePasswordChange} className="profile-form">
                                <div className="form-field">
                                    <label>Current Password</label>
                                    <input
                                        type="password"
                                        value={formData.currentPassword}
                                        onChange={(e) => setFormData({...formData, currentPassword: e.target.value})}
                                        placeholder="Enter current password"
                                    />
                                </div>
                                <div className="form-field">
                                    <label>New Password</label>
                                    <input
                                        type="password"
                                        value={formData.newPassword}
                                        onChange={(e) => setFormData({...formData, newPassword: e.target.value})}
                                        placeholder="Enter new password"
                                    />
                                </div>
                                <div className="form-field">
                                    <label>Confirm Password</label>
                                    <input
                                        type="password"
                                        value={formData.confirmPassword}
                                        onChange={(e) => setFormData({...formData, confirmPassword: e.target.value})}
                                        placeholder="Confirm new password"
                                    />
                                </div>
                                <button type="submit" className="save-button">
                                    Update Password
                                </button>
                            </form>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Profile;