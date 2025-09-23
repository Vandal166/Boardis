import { Outlet, useNavigate } from 'react-router-dom';
import Header from '../components/Header';
import Footer from '../components/Footer';
import { useTranslation } from 'react-i18next';
import { useKeycloak } from '@react-keycloak/web';

function Dashboard()
{
    const { keycloak } = useKeycloak();
    const navigate = useNavigate();
    const { t } = useTranslation();

    return (
        <div className="min-h-screen bg-gray-100 text-gray-900 font-sans">

            <Header />

            <div className="flex min-h-[calc(100vh-4rem)] shadow-xl">
                {/* Sidebar */}
                <aside className="w-64 bg-gray-800 text-white p-6 shadow-md">
                    <h3 className="text-xl font-semibold mb-4">{t('dashboardNavigation')}</h3>
                    <ul className="space-y-2">
                        <li>
                            <button
                                onClick={() => navigate('/dashboard')}
                                className="group w-full flex justify-between items-center text-left px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 rounded-md transition"
                            >
                                {t('dashboardHome')}
                                <span
                                    className="inline-block ml-2 text-3xl leading-none opacity-0 transform translate-x-2 group-hover:opacity-100 group-hover:translate-x-0 transition-all duration-200"
                                >
                                    ›
                                </span>
                            </button>
                        </li>
                        <li>
                            <button
                                onClick={() => keycloak.accountManagement()}
                                className="group w-full flex justify-between items-center text-left px-4 py-2 text-sm bg-transparent hover:bg-gray-700 rounded-md transition"
                            >
                                {t('dashboardProfile')}
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
                <main className="flex-1">
                    <Outlet />
                </main>

            </div>
            <Footer />
        </div>
    );
}

export default Dashboard;