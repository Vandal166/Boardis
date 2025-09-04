import { useKeycloak } from '@react-keycloak/web';
import { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import Header from '../components/Header';
import Footer from '../components/Footer';
import Boards from './Boards';

interface Board
{
    id: string;
    title: string;
    description?: string;
    wallpaperImageId?: string;
}

function Dashboard()
{
    const { keycloak, initialized } = useKeycloak();
    const navigate = useNavigate();
    const [, setBoards] = useState<Board[]>([]);
    const [, setError] = useState<string | null>(null);
    const [, setIsLoading] = useState(false);

    useEffect(() =>
    {
        if (initialized && keycloak.authenticated && keycloak.token)
        {
            const fetchBoards = async () =>
            {
                setIsLoading(true);
                try
                {
                    const response = await axios.get('/api/boards', {
                        headers: { Authorization: `Bearer ${keycloak.token}` },
                    });
                    setBoards(response.data);
                    setError(null);
                } catch (error)
                {
                    console.error('Failed to fetch boards:', error);
                    setError('Failed to load boards. Please try again later.');
                } finally
                {
                    setIsLoading(false);
                }
            };
            fetchBoards();
        } else if (initialized && !keycloak.authenticated)
        {
            navigate('/'); // Redirect to home if not authenticated
        }
    }, [initialized, keycloak, navigate]);

    return (
        <div className="min-h-screen bg-gray-100 text-gray-900 font-sans">

            <Header />

            <div className="flex min-h-[calc(100vh-4rem)] shadow-xl">
                {/* Sidebar */}
                <aside className="w-64 bg-gray-800 text-white p-6 shadow-md">
                    <h3 className="text-xl font-semibold mb-4">Navigation</h3>
                    <ul className="space-y-2">
                        <li>
                            <button
                                onClick={() => navigate('/dashboard')}
                                className="group w-full flex justify-between items-center text-left px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 rounded-md transition"
                            >
                                Home
                                <span
                                    className="inline-block ml-2 text-3xl leading-none opacity-0 transform translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all duration-200"
                                >
                                    ›
                                </span>
                            </button>
                        </li>
                        <li>
                            <button
                                onClick={() => <Boards />}
                                className="group w-full flex justify-between items-center text-left px-4 py-2 text-sm bg-transparent hover:bg-gray-700 rounded-md transition"
                            >
                                Boards
                                <span
                                    className="inline-block ml-2 text-3xl leading-none opacity-0 transform translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all duration-200"
                                >
                                    ›
                                </span>
                            </button>
                        </li>
                        <li>
                            <button
                                onClick={() => navigate('/profile')}
                                className="group w-full flex justify-between items-center text-left px-4 py-2 text-sm bg-transparent hover:bg-gray-700 rounded-md transition"
                            >
                                Profile
                                <span
                                    className="inline-block ml-2 text-3xl leading-none opacity-0 transform translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all duration-200"
                                >
                                    ›
                                </span>
                            </button>
                        </li>
                        <li>
                            <button
                                onClick={() => navigate('/settings')}
                                className="group w-full flex justify-between items-center text-left px-4 py-2 text-sm bg-transparent hover:bg-gray-700 rounded-md transition"
                            >
                                Settings
                                <span
                                    className="inline-block ml-2 text-3xl leading-none opacity-0 transform translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all duration-200"
                                >
                                    ›
                                </span>
                            </button>
                        </li>
                    </ul>
                </aside>
                {/* Main Content */}
                <Boards />
            </div>
            <Footer />
        </div>
    );
}

export default Dashboard;