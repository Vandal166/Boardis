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

    const { t, i18n } = useTranslation();

    // Language dropdown state
    const [langDropdownOpen, setLangDropdownOpen] = useState(false);
    const langBtnRef = useRef<HTMLButtonElement>(null);
    const langMenuRef = useRef<HTMLDivElement>(null);

    useEffect(() =>
    {
        const handleClickOutside = (event: MouseEvent) =>
        {
            if (
                langMenuRef.current &&
                !langMenuRef.current.contains(event.target as Node) &&
                langBtnRef.current &&
                !langBtnRef.current.contains(event.target as Node)
            )
            {
                setLangDropdownOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleLogin = () => keycloak.login({ redirectUri: window.location.origin + '/' });
    const handleRegister = () => keycloak.register({ redirectUri: window.location.origin + '/' });
    const handleLogout = () => keycloak.logout({ redirectUri: window.location.origin + '/' });

    // Helper for language code
    const getLangCode = (lang: string) =>
    {
        if (lang === 'en') return 'EN';
        if (lang === 'pl') return 'PL';
        return lang.toUpperCase();
    };

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
                {/* Language Switcher with custom SVG icon */}
                <div className="relative">
                    <button
                        ref={langBtnRef}
                        onClick={() => setLangDropdownOpen(v => !v)}
                        className="bg-gray-700 text-white px-2 py-1 rounded-md focus:outline-none flex items-center gap-2"
                        aria-label="Select language"
                        type="button"
                    >
                        {/* Inline SVG for language icon */}
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-6 h-6">
                            <path strokeLinecap="round" strokeLinejoin="round" d="m10.5 21 5.25-11.25L21 21m-9-3h7.5M3 5.621a48.474 48.474 0 0 1 6-.371m0 0c1.12 0 2.233.038 3.334.114M9 5.25V3m3.334 2.364C11.176 10.658 7.69 15.08 3 17.502m9.334-12.138c.896.061 1.785.147 2.666.257m-4.589 8.495a18.023 18.023 0 0 1-3.827-5.802" />
                        </svg>
                        <span className="hidden sm:inline">{getLangCode(i18n.language)}</span>
                    </button>
                    {langDropdownOpen && (
                        <div
                            ref={langMenuRef}
                            className="absolute left-0 mt-2 w-32 bg-white rounded-lg shadow-lg py-2 z-20"
                        >
                            <button
                                className={`w-full text-left px-4 py-2 text-sm hover:bg-gray-100 ${i18n.language === 'en' ? 'font-bold text-blue-700' : 'text-gray-800'}`}
                                onClick={() => { i18n.changeLanguage('en'); setLangDropdownOpen(false); }}
                            >
                                English
                            </button>
                            <button
                                className={`w-full text-left px-4 py-2 text-sm hover:bg-gray-100 ${i18n.language === 'pl' ? 'font-bold text-blue-700' : 'text-gray-800'}`}
                                onClick={() => { i18n.changeLanguage('pl'); setLangDropdownOpen(false); }}
                            >
                                Polish
                            </button>
                        </div>
                    )}
                </div>
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