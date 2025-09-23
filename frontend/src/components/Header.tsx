import { useKeycloak } from '@react-keycloak/web';
import { useNavigate, useMatch } from 'react-router-dom';
import { useRef, useState, useEffect } from 'react';
import { UserCircleIcon } from '@heroicons/react/24/solid';
import logo from '../assets/logo.png';
import api from '../api';
import { useBoardSignalR } from '../communication/BoardSignalRProvider';
import { useTranslation } from 'react-i18next';

function Header()
{
    const { keycloak, initialized } = useKeycloak();
    const navigate = useNavigate();
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const avatarRef = useRef<HTMLButtonElement>(null);
    const menuRef = useRef<HTMLDivElement>(null);

    // Detect if we're on a board route and fetch the board title
    const boardMatch = useMatch('/dashboard/board/:boardId/*');
    const boardId = boardMatch?.params.boardId;
    const [boardTitle, setBoardTitle] = useState<string | null>(null);

    const boardHubConnection = useBoardSignalR();
    useEffect(() =>
    {
        let active = true;
        if (!boardId)
        {
            setBoardTitle(null);
            return;
        }
        const fetchBoardTitle = async () =>
        {
            if (!initialized || !keycloak.authenticated || !keycloak.token)
                return;

            await api.get(`/api/boards/${boardId}`)
                .then(res => { if (active) setBoardTitle(res.data?.title ?? null); })
                .catch(() => { if (active) setBoardTitle(null); });
        };

        fetchBoardTitle();

        // SignalR listener
        const handleBoardUpdated = (updatedBoardId: string) =>
        {
            if (updatedBoardId === boardId)
            {
                fetchBoardTitle();
            }
        };

        boardHubConnection.on('BoardUpdated', handleBoardUpdated);

        return () =>
        {
            active = false;
            boardHubConnection.off('BoardUpdated', handleBoardUpdated);
        };
    }, [boardId, initialized, keycloak.authenticated, keycloak.token, boardHubConnection]);

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

    const { t, i18n } = useTranslation();

    return (
        <header className="sticky top-0 z-50 w-full flex justify-between items-center p-4 bg-gray-800 shadow-2xl">
            <button
                className="flex items-center focus:outline-none !shadow-none"
                onClick={() => navigate('/')}
                aria-label="Go to home"
                type="button"
            >
                <img src={logo} alt="Boardis Logo" className="w-12 h-12 mr-3" />
                <h1 className="text-2xl font-bold text-white transition-transform duration-200 hover:-translate-y-2">
                    {t('headerAppName')}
                </h1>
                {boardTitle && (
                    <span className="ml-3 text-white/90 text-lg truncate max-w-[25vw]">
                        | {boardTitle}
                    </span>
                )}
            </button>
            <div className="flex items-center space-x-4">
                {/* Language Switcher */}
                <select
                    value={i18n.language}
                    onChange={e => i18n.changeLanguage(e.target.value)}
                    className="bg-gray-700 text-white px-2 py-1 rounded-md focus:outline-none"
                    style={{ minWidth: 70 }}
                    aria-label="Select language"
                >
                    <option value="en">English</option>
                    <option value="pl">Polski</option>
                </select>
                {initialized ? (
                    keycloak.authenticated ? (
                        <>
                            <button
                                onClick={() => navigate('/dashboard')}
                                className="bg-blue-600 text-2xl text-white px-4 py-1 rounded-md hover:bg-blue-700 hover:scale-105 hover:shadow-xl transition-all duration-150 flex items-center"
                            >
                                {t('headerDashboard')}
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
                                            onClick={() => keycloak.accountManagement()}
                                            className="w-full text-left px-4 py-2 text-sm text-gray-800 hover:bg-gray-100"
                                        >
                                            {t('headerYourProfile')}
                                        </button>
                                        <button
                                            onClick={() => navigate('/dashboard')}
                                            className="w-full text-left px-4 py-2 text-sm text-gray-800 hover:bg-gray-100"
                                        >
                                            {t('headerYourBoards')}
                                        </button>
                                        <button
                                            onClick={handleLogout}
                                            className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gray-100"
                                        >
                                            {t('headerLogout')}
                                        </button>
                                    </div>
                                )}
                            </div>
                        </>
                    ) : (
                        <>
                            <button
                                onClick={handleLogin}
                                className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition"
                            >
                                {t('headerLogin')}
                            </button>
                            <button
                                onClick={handleRegister}
                                className="bg-orange-500 text-white px-4 py-2 rounded-md hover:bg-orange-600 transition"
                            >
                                {t('headerRegister')}
                            </button>
                        </>
                    )
                ) : (
                    <>
                        <div className="animate-pulse bg-gray-600 h-10 w-20 rounded-md"></div>
                        <div className="animate-pulse bg-gray-600 h-10 w-20 rounded-md"></div>
                    </>
                )}
            </div>
        </header>
    );
}

export default Header;