import { useState } from 'react';
import './Login.css';

function Login({ onLogin }) {
    const [userName, setUserName] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = (e) => {
        e.preventDefault();

        if (!userName.trim()) {
            setError('Please enter your name');
            return;
        }

        onLogin(userName);
    };

    return (
        <div className="login-container">
            <div className="login-card">
                <h2>Welcome to RT Chat</h2>
                <p>Please enter your name to continue</p>

                <form onSubmit={handleSubmit}>
                    <div className="input-group">
                        <label htmlFor="userName">Your Name</label>
                        <input
                            type="text"
                            id="userName"
                            value={userName}
                            onChange={(e) => {
                                setUserName(e.target.value);
                                setError('');
                            }}
                            placeholder="Enter your name"
                            autoFocus
                        />
                        {error && <div className="error-message">{error}</div>}
                    </div>

                    <button
                        type="submit"
                        className="login-button"
                        disabled={!userName.trim()}
                    >
                        Enter Chat
                    </button>
                </form>
            </div>
        </div>
    );
}

export default Login;