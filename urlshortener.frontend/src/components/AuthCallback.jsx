import { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../AuthContext';

const AuthCallback = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { login } = useAuth();

    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const token = params.get('token');
        const error = params.get('error');
        
        if (error) {
            console.error('Authentication error:', error);
            navigate('/login?error=' + encodeURIComponent(error));
            return;
        }
        
        if (token) {
            localStorage.setItem('user', JSON.stringify({ token }));
            login({ token });
            navigate('/');
        } else {
            navigate('/login');
        }
    }, [location, navigate, login]);

    return <div>Processing login...</div>;
};

export default AuthCallback;