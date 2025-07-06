import { useState } from 'react';
import './App.css';
import Chat from "./Chat/Chat.js";
import Login from "./Login/Login.js";

function App() {
    const [userName, setUserName] = useState('');
    const [isLoggedIn, setIsLoggedIn] = useState(false);

    const handleLogin = (name) => {
        setUserName(name);
        setIsLoggedIn(true);
    };

    const handleLogout = () => {
        setIsLoggedIn(false);
        setUserName('');
    };

    return (
        <div className="app">
            {isLoggedIn ? (
                <Chat userName={userName} onLogout={handleLogout} />
            ) : (
                <Login onLogin={handleLogin} />
            )}
        </div>
    );
}

export default App;