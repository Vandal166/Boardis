import { useKeycloak } from '@react-keycloak/web';
import { useNavigate } from 'react-router-dom';
import { useRef, useState, useEffect } from 'react';
import { UserCircleIcon } from '@heroicons/react/24/solid';
import logo from '../assets/logo.png';

function Header()
{
    const { keycloak, initialized } = useKeycloak();
    const navigate = useNavigate();
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const avatarRef = useRef<HTMLButtonElement>(null);
    const menuRef = useRef<HTMLDivElement>(null);

    useEffect(() =>
    {
        const handleClickOutside = (event: MouseEvent) =>
        {
            if (
                menuRef.current &&
                !menuRef.current.contains(event.target as Node) &&
                avatarRef.current &&
                !avatarRef.current.contains(event.target as Node)
            )
            {
                setDropdownOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleLogin = () => keycloak.login({ redirectUri: window.location.origin + '/' });
    const handleRegister = () => keycloak.register({ redirectUri: window.location.origin + '/' });
    const handleLogout = () => keycloak.logout({ redirectUri: window.location.origin + '/' });

    return (
        <header className="sticky top-0 z-50 w-full flex justify-between items-center p-4 bg-gray-800 shadow-2xl">
            <button
                className="flex items-center focus:outline-none"
                onClick={() => navigate('/')}
                aria-label="Go to home"
                type="button"
            >
                <img src={logo} alt="Boardis Logo" className="w-12 h-12 mr-3" />
                <h1 className="text-2xl font-bold text-white transition-transform duration-200 hover:-translate-y-2">
                    Boardis
                </h1>
            </button>
            {initialized ? (
                keycloak.authenticated ? (
                    <div className="flex items-center space-x-4">
                        <button
                            onClick={() => navigate('/dashboard')}
                            className="bg-blue-600 text-2xl text-white px-4 py-1 rounded-md hover:bg-blue-700 hover:scale-105 hover:shadow-xl transition-all duration-150 flex items-center"
                        >
                            Dashboard
                        </button>
                        <div className="relative">
                            <button
                                ref={avatarRef}
                                onClick={() => setDropdownOpen(!dropdownOpen)}
                                className="flex items-center justify-center focus:outline-none"
                                aria-label="User menu"
                            >
                                <UserCircleIcon className="w-10 h-10 text-white" />
                            </button>
                            {dropdownOpen && (
                                <div
                                    ref={menuRef}
                                    className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg py-2 z-10"
                                >
                                    <button
                                        onClick={() => navigate('/profile')}
                                        className="w-full text-left px-4 py-2 text-sm text-gray-800 hover:bg-gray-100"
                                    >
                                        Your Profile
                                    </button>
                                    <button
                                        onClick={() => navigate('/dashboard')}
                                        className="w-full text-left px-4 py-2 text-sm text-gray-800 hover:bg-gray-100"
                                    >
                                        Your Boards
                                    </button>
                                    <button
                                        onClick={handleLogout}
                                        className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gray-100"
                                    >
                                        Logout
                                    </button>
                                </div>
                            )}
                        </div>
                    </div>
                ) : (
                    <div className="flex space-x-4">
                        <button
                            onClick={handleLogin}
                            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition"
                        >
                            Login
                        </button>
                        <button
                            onClick={handleRegister}
                            className="bg-orange-500 text-white px-4 py-2 rounded-md hover:bg-orange-600 transition"
                        >
                            Register
                        </button>
                    </div>
                )
            ) : (
                <div className="flex space-x-4">
                    <div className="animate-pulse bg-gray-600 h-10 w-20 rounded-md"></div>
                    <div className="animate-pulse bg-gray-600 h-10 w-20 rounded-md"></div>
                </div>
            )}
        </header>
    );
}

export default Header;